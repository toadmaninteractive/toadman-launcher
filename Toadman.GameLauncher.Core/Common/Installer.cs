using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebProtocol;
using System.IO;
using System.IO.Compression;
using NLog;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Toadman.GameLauncher.Core
{
    public class Installer
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public Installer() { }

        public async Task DownloadAndDecompresAsync(List<GameFile> filesInfo,
            string branchRootUrl,
            string localGameFolder,
            IProgress<InstallProgress> progress,
            CancellationToken token)
        {
            var smallFileModels = filesInfo.Where(x => x.CompressedSize < Constants.ChunkingFileSize).ToList();
            var bigFileModels = filesInfo.Where(x => x.CompressedSize >= Constants.ChunkingFileSize).ToList();

            try
            {
                using (SemaphoreSlim semaphore = new SemaphoreSlim(10, 10))
                {
                    var smallFileDownloadTasks =
                        GetSmallFileDownloadTasks(smallFileModels, branchRootUrl, localGameFolder, semaphore, progress, token);
                    
                    await Task.WhenAll(smallFileDownloadTasks);
                    
                    //============ Chunks ============
                    for (int i = 0; i < bigFileModels.Count; i++)
                    {
                        var bigFileModel = bigFileModels[i];
                        var localName = Path.Combine(localGameFolder, bigFileModel.RelativePath.Replace('/', '\\'));
                        var localCompressedName = Path.Combine(localGameFolder, bigFileModel.RelativeCompressedPath.Replace('/', '\\'));
                        var directoryName = Path.GetDirectoryName(localName);
                        Directory.CreateDirectory(directoryName);

                        if (!Utils.CheckFileArchive(localCompressedName, bigFileModel.CompressedSize))
                        {
                            var chunkNameDict = GetChunkDict(bigFileModel.CompressedSize, localName);
                            var chunkDownloadTasks = GetChunksDownloadTasks(
                                chunkNameDict,
                                bigFileModel.RelativeCompressedPath,
                                branchRootUrl, semaphore, progress, token);

                            //phaseProgress.Report(ProcessesingPhase.Downloading);
                            await Task.WhenAll(chunkDownloadTasks);

                            //phaseProgress.Report(ProcessesingPhase.MergeChunks);
                            await Task.Run(() =>
                            {
                                MergeChunksToFile(chunkNameDict, localCompressedName, token);
                            });
                        }

                        //phaseProgress.Report(ProcessesingPhase.Decompress);
                        await Decompress(localName, localCompressedName, token);
                    }
                }                
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                    throw new OperationCanceledException();

                var baseEx = ex.GetBaseException();
                if (baseEx is IOException)
                    throw ex.GetBaseException();

                throw;
            }
        }

        private List<Task> GetSmallFileDownloadTasks(
            List<GameFile> smallFileModels,
            string branchRootUrl,
            string localGameFolder,
            SemaphoreSlim semaphore,
            IProgress<InstallProgress> progress,
            CancellationToken token)
        {
            return smallFileModels.Select(async fileInfo =>
            {
                try
                {
                    await semaphore.WaitAsync();

                    if (token.IsCancellationRequested)
                        return;

                    var localCompressedName = Path.Combine(localGameFolder, fileInfo.RelativeCompressedPath.Replace('/', '\\'));
                    var localName = Path.Combine(localGameFolder, fileInfo.RelativePath.Replace('/', '\\'));

                    if (!Utils.CheckFileArchive(localCompressedName, fileInfo.CompressedSize))
                    {
                        var tempDownloadName = $"{localCompressedName}{Constants.InvalidDownloadExtension}";
                        Directory.CreateDirectory(Path.GetDirectoryName(localName));

                        using (var webClient = new LauncherWebClient(progress, token))
                        {
                            token.Register(webClient.CancelAsync);
                            await webClient.DownloadFileTaskAsync(new Uri($"{branchRootUrl}{fileInfo.RelativeCompressedPath}"), tempDownloadName);
                        }

                        File.Delete(localCompressedName);
                        File.Move(tempDownloadName, localCompressedName);
                    }
                    else
                        progress.Report(new InstallProgress(localCompressedName, fileInfo.CompressedSize, fileInfo.CompressedSize, FileProcessingStage.Complete));

                    await Decompress(localName, localCompressedName, token);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && 404 == (int)response.StatusCode)
                        logger.Error($"File {fileInfo.RelativePath} not found on server");

                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();
        }

        private static List<Task> GetChunksDownloadTasks(
            Dictionary<int, string> chunksDict,
            string fileRelativeCompressedPath,
            string branchRootUrl,
            SemaphoreSlim semaphore,
            IProgress<InstallProgress> progress,
            CancellationToken token)
        {
            return chunksDict.Select(async chunkName =>
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (token.IsCancellationRequested)
                        return;

                    await DownloadChunk(
                        chunkName.Value,
                        fileRelativeCompressedPath,
                        chunkName.Key, branchRootUrl, progress, token);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && 404 == (int)response.StatusCode)
                    { }

                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();
        }

        private static async Task DownloadChunk(
            string chunkName,
            string fileRelativeCompressedPath,
            int index,
            string branchRootUrl,
            IProgress<InstallProgress> progress,
            CancellationToken token)
        {
            if (File.Exists(chunkName))
            {
                var fileInfo = new FileInfo(chunkName);
                progress.Report(
                    new InstallProgress(chunkName, fileInfo.Length, fileInfo.Length, FileProcessingStage.Complete));
                return;
            }

            using (var webClient = new LauncherWebClient(Constants.ChunkSize * index, Constants.ChunkSize * (index + 1) - 1, progress, token))
            {
                var tempDownloadName = $"{chunkName}{Constants.InvalidDownloadExtension}";
                var fileAdress = $"{branchRootUrl}{fileRelativeCompressedPath}";

                token.Register(webClient.CancelAsync);
                await webClient.DownloadFileTaskAsync(new Uri(fileAdress), tempDownloadName);
                File.Delete(chunkName);
                File.Move(tempDownloadName, chunkName);
            }
        }

        private static void MergeChunksToFile(Dictionary<int, string> chunks, string destinationFilePath, CancellationToken token)
        {
            var tempDestinationFilePath = $"{destinationFilePath}{Constants.InvalidMergeChunks}";

            using (Stream destinationStream = File.OpenWrite(tempDestinationFilePath))
            {
                foreach (var chunk in chunks.OrderBy(x => x.Key))
                {
                    token.ThrowIfCancellationRequested();

                    using (Stream tempFileStream = File.OpenRead(chunk.Value))
                    {
                        tempFileStream.CopyTo(destinationStream);
                    }
                }
            }

            File.Delete(destinationFilePath);
            File.Move(tempDestinationFilePath, destinationFilePath);
            File.Delete(tempDestinationFilePath);

            foreach (var chunk in chunks)
                File.Delete(chunk.Value);
        }

        private static async Task Decompress(string localName, string localCompressedName, CancellationToken token)
        {
            var tempDecompressedName = $"{localName}{Constants.InvalidCompressedExtension}";
            await GZipDecompressAndSave(localCompressedName, tempDecompressedName, token);

            File.Delete(localName);
            File.Move(tempDecompressedName, localName);
            File.Delete(localCompressedName);
        }

        private static async Task GZipDecompressAndSave(
            string compressedName,
            string destinationName,
            CancellationToken ct)
        {
            var fileToDecompress = new FileInfo(compressedName);
            using (FileStream compressedFileStream = fileToDecompress.OpenRead())
            {
                using (FileStream decompressedFileStream = File.Create(destinationName))
                {
                    using (GZipStream GZipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                    {
                        await GZipStream.CopyToAsync(decompressedFileStream, (int)Constants.ChunkSize, ct);
                    }
                }
            }
        }

        private Dictionary<int, string> GetChunkDict(long fileCompressedSize, string fileLocalName)
        {
            var chunksDict = new Dictionary<int, string>();

            for (int j = 0; Constants.ChunkSize * j < fileCompressedSize; j++)
            {
                var tempChunkName = GetChunkName(fileLocalName, j);
                chunksDict.Add(j, tempChunkName);
            }

            return chunksDict;
        }

        private string GetChunkName(string localFileName, int index)
        {
            return $"{localFileName}{Constants.ChunkExtension}_{index + 1}{Constants.ChunkExtension}";
        }
    }
}