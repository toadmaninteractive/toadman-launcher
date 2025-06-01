using System;

namespace Toadman.GameLauncher.Core
{
    [Serializable]
    public class GameInstallData
    {
        public string GameGuid { get; set; }
        public string GameDirectory { get; set; }
        public string BranchName { get; set; }
        public string BranchBuild { get; set; }
        public string BranchExePath { get; set; }
    }
}