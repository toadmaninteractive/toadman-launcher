using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Toadman.GameLauncher.Core
{
    public class ApplicationUpdater : NotifyPropertyChanged, ILoadingUpdate
    {
        public long InstallationFileSize
        {
            get { return installationFileSize; }
            set { SetField(ref installationFileSize, value); }
        }
        public long DownloadProgress
        {
            get { return downloadProgress; }
            set { SetField(ref downloadProgress, value); }
        }
        public string UpdateFileName;
        private long installationFileSize = 0;
        private long downloadProgress = 0;
        private readonly DirectoryInfo tempPath;
        private CancellationTokenSource cts;
        private Logger logger = LogManager.GetCurrentClassLogger();

        public ApplicationUpdater()
        {
            tempPath = Directory.CreateDirectory(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Temp", 
                    Constants.TempFolderName));
        }

        public async Task<string> GetRemoteRevisionAsync(ApplicationUpdateChannel channel)
        {
            using (WebClient webClient = new WebClient())
            {
                var channelUrl = Constants.UpdateUrl + channel.ToString().ToLower();
                return (await webClient.DownloadStringTaskAsync(channelUrl + "/rev.conf")).Trim();
            }
        }

        public bool CheckForSetupFile(ApplicationUpdateChannel channel, string remoteRevision)
        {
            return true;
            
            /*
            if (File.Exists(".ignoreupdate"))
                return true;

            var tempFileName = GetTempFileName(remoteRevision);
            var isTempFileExist = File.Exists(tempFileName);
            if (isTempFileExist)
                UpdateFileName = tempFileName;
            return isTempFileExist;
            */
        }

        public async Task DownloadUpdatesAsync(ApplicationUpdateChannel channel, string remoteRevision)
        {
            InstallationFileSize = 0;
            DownloadProgress = 0;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    Directory
                        .GetFiles(tempPath.FullName)
                        .ToList()
                        .ForEach(f => File.Delete(f));

                    var tempFileName = GetTempFileName(remoteRevision);
                    using (cts = new CancellationTokenSource())
                    {
                        webClient.DownloadProgressChanged += WebClientDownloadProgressChanged;
                        cts.Token.Register(webClient.CancelAsync);

                        var downloadFileName = Path.Combine(tempPath.FullName, "toadman_launcher_setup.download");
                        await webClient.DownloadFileTaskAsync(
                            $"{Constants.UpdateUrl}{channel.ToString().ToLower()}/{Constants.SetupFileName}", downloadFileName);

                        File.Move(downloadFileName, tempFileName);
                    }

                    UpdateFileName = tempFileName;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        public void CancelDownload()
        {
            UpdateFileName = string.Empty;
            cts?.Cancel();
        }

        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (InstallationFileSize == 0)
                InstallationFileSize = e.TotalBytesToReceive;

            var userState = (TaskCompletionSource<object>)e.UserState;
            if (userState.Task.Status != TaskStatus.RanToCompletion)
                DownloadProgress = e.BytesReceived;
        }

        private string GetTempFileName(string remoteRevision)
        {
            return Path.Combine(tempPath.FullName, $"{Constants.TempSetupFileName}{remoteRevision}.exe");
        }
    }
}