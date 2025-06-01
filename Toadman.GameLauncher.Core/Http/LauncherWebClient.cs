using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Toadman.GameLauncher.Core
{
    public class LauncherWebClient : WebClient
    {
        private CancellationToken token;
        private IProgress<InstallProgress> progress;
        private Uri uri;

        private long from;
        private long to;
        private bool isChunkDownload;

        public LauncherWebClient(IProgress<InstallProgress> progress, CancellationToken token)
        {
            this.progress = progress;
            this.token = token;
            isChunkDownload = false;
        }

        public LauncherWebClient(long from, long to, IProgress<InstallProgress> progress, CancellationToken token)
        {
            this.progress = progress;
            this.token = token;
            this.from = from;
            this.to = to;
            isChunkDownload = true;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            this.uri = uri;
            HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
            if (isChunkDownload)
                w.AddRange(from, to);

            return w;
        }

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (token.IsCancellationRequested)
                CancelAsync();

            var userState = (TaskCompletionSource<object>)e.UserState;

            var id = isChunkDownload ? $"{uri.AbsolutePath}{from}" : uri.AbsolutePath;

            progress.Report(
                new InstallProgress(id, e.BytesReceived, e.TotalBytesToReceive,
                userState.Task.Status == TaskStatus.RanToCompletion ? FileProcessingStage.Complete : FileProcessingStage.Processing));

            base.OnDownloadProgressChanged(e);
        }
    }
}