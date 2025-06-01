using System;
using System.Threading;
using System.Threading.Tasks;
using WebProtocol;

namespace Toadman.GameLauncher.Core
{
    /// <summary>
    /// Using in GameViewModel for direct binging Model-property to GUI control dependence properties
    /// </summary>
    public interface IGameModel
    {
        event Action<GameProcessingPhase> PhaseChange;
        event Action<GameInstallStatus> InstallStatusChange;
        event Action<GameState> GameStateChange;

        string Title { get; set; }
        string Description { get; set; }
        string DiscordUrl { get; set; }
        Price Price { get; }
        string CurrentBranchName { get; }
        string[] AvailableBranchNames { get; }
        string LocalPath { get; set; }
        long InstallationSize { get; }
        long DownloadSize { get; }
        GameInstallStatus InstallStatus { get; }
        GameState State { get; set; }
        GameProcessingPhase ProcessingPhase { get; }
        long ProgressMax { get; }
        string Guid { get; set; }

        Task Purchase(string sessionId);
        Task Install(string sessionId, IProgress<InstallProgress> progress, CancellationToken token);
        Task Update(string sessionId, IProgress<InstallProgress> progress, CancellationToken cancellationToken);
        Task SwitchBranch(string sessionId, IProgress<InstallProgress> progress, CancellationToken cancellationToken, string targetBranch);
        Task Uninstall();
        Task TryRemoveGameFilesAsync();
        void Launch();
        void SetGameInfo(GameItem newGameInfo);
    }
}