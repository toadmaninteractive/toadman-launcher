using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toadman.GameLauncher.Core
{
    public interface ILoadingUpdate
    {
        long DownloadProgress { get; set; }
        long InstallationFileSize { get; set; }
        void CancelDownload();
    }
}