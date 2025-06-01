using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebProtocol;

namespace Toadman.GameLauncher.Core
{
    public partial class GameModel
    {
        private async Task<GameManifest> DownloadManifest(
           string targetBranchName,
           string sessionId,
           CancellationToken cancellationToken)
        {
            try
            {
                ProcessingPhase = GameProcessingPhase.ManifestUpdating;
                return await HeliosApi.Provider.GetGameManifestAsync(sessionId, gameInfo.Guid, targetBranchName);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                    throw new OperationCanceledException();

                throw;
            }
        }

        private async Task<List<GameFile>> GetExpiredLocalFiles(
           List<GameFile> remoteFilesInfo,
           IProgress<InstallProgress> progress,
           CancellationToken ct)
        {
            ProgressMax = remoteFilesInfo.Count;
            ProcessingPhase = GameProcessingPhase.FilesIntegrityChecking;
            var expiredFiles = new List<GameFile>();
            
            await Task.Run(() =>
            {
                for (int i = 0; i < remoteFilesInfo.Count; i++)
                {
                    var remoteFile = remoteFilesInfo[i];
                    ct.ThrowIfCancellationRequested();
                    var diffKind = CheckIntedrityLocalFile(remoteFile, LocalPath);
                    if (diffKind != DiffKind.Valid)
                        expiredFiles.Add(remoteFile);

                    progress.Report(
                        new InstallProgress(remoteFile.RelativePath, 1, 1, FileProcessingStage.Complete));
                }
            });

            return expiredFiles;
        }

        private DiffKind CheckIntedrityLocalFile(GameFile remoteFile, string localPath)
        {
            var localName = Path.Combine(localPath, remoteFile.RelativePath);
            var localCompressedName = Path.Combine(localPath, remoteFile.RelativeCompressedPath);

            if (File.Exists(localName))
            {
                using (var stream = File.OpenRead(localName))
                {
                    if (remoteFile.Size != stream.Length)
                        return DiffKind.Modify;

                    stream.Position = 0;
                    return remoteFile.Md5 != MD5(stream) ? DiffKind.Modify : DiffKind.Valid;
                }
            }

            return DiffKind.Add;
        }

        private string MD5(FileStream stream)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var data = md5.ComputeHash(stream);

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        //Call on Constructor, Purchase or UnlockBranch
        public void SetGameInfo(GameItem newGameInfo)
        {
            gameInfo = newGameInfo;
            Title = gameInfo.Title;
            Guid = gameInfo.Guid;
            Description = gameInfo.Description;
            DiscordUrl = gameInfo.DiscordUrl;

            ResetCurrentBranch();
        }

        private void SetDefaultBranch()
        {
            // Fallback to first branch if there is no default one
            CurrentBranch = gameInfo.Branches.SingleOrDefault(x => x.IsDefault);

            if (CurrentBranch == null)
                logger.Warn($"No default branch set for game <{Title}> identified by key <{Guid}>");

            CurrentBranch = gameInfo.Branches.First();
        }

        /// <summary>
        /// Call after "gameInfo" property has updated
        /// </summary>
        private void ResetCurrentBranch()
        {
            var currentBranchName = CurrentBranch?.Name;
            var actualCurrentBranchInfo = gameInfo.Branches.SingleOrDefault(x => x.IsDefault);

            if (actualCurrentBranchInfo == null)
                SetDefaultBranch();
            else
                CurrentBranch = actualCurrentBranchInfo;
        }   
    }
}