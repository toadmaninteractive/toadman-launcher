using System;

namespace Toadman.GameLauncher.Core
{
    public struct InstallProgress
    {
        public InstallProgress(string url, long bytesProcessed, long bytesTotal, FileProcessingStage stage)
        {
            Url = url;
            BytesProcessed = bytesProcessed;
            BytesTotal = bytesTotal;
            Stage = stage;
        }

        public string Url;
        public long BytesProcessed;
        public long BytesTotal;
        public FileProcessingStage Stage;
    }
}