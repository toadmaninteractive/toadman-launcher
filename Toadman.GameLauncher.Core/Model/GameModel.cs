using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WebProtocol;

namespace Toadman.GameLauncher.Core
{
    public partial class GameModel : NotifyPropertyChanged, IGameModel
    {
        public event Action<GameProcessingPhase> PhaseChange;
        public event Action<GameInstallStatus> InstallStatusChange;
        public event Action<GameState> GameStateChange; 

        public string Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DiscordUrl { get; set; }
        public Price Price => gameInfo.Price;
        public string CurrentBranchName => CurrentBranch.Name;
        public string[] AvailableBranchNames => gameInfo.Branches.Select(x => x.Name).ToArray();
        public long InstallationSize => CurrentBranch.Size;
        public long DownloadSize => CurrentBranch.CompressedSize;
        public GameInstallStatus InstallStatus
        {
            get { return installStatus; }
            private set
            {
                SetField(ref installStatus, value);
                InstallStatusChange?.Invoke(value);
            }
        }
        public GameState State
        {
            get { return state; }
            set
            {
                SetField(ref state, value);
                GameStateChange?.Invoke(value);
            }
        }
        public GameProcessingPhase ProcessingPhase
        {
            get { return processingPhase; }
            private set
            {
                SetField(ref processingPhase, value);
                PhaseChange?.Invoke(value);
            }
        }
        /// <summary>
        /// Game root path
        /// </summary>
        public string LocalPath { get; set; }
        public GameBranchItem CurrentBranch 
        {
            get { return currentBranch; }
            set
            {
                SetField(ref currentBranch, value);

                RaisePropertyChanged(nameof(CurrentBranchName));
                RaisePropertyChanged(nameof(InstallationSize));
                RaisePropertyChanged(nameof(DownloadSize));
            }
        }
        public long ProgressMax { get; private set; }

        private Logger logger = LogManager.GetCurrentClassLogger();
        private GameBranchItem currentBranch;
        private GameInstallStatus installStatus;
        private GameState state;
        private GameProcessingPhase processingPhase = GameProcessingPhase.Idleness;
        private GameItem gameInfo;
        private Installer installer = new Installer();

        public GameModel(GameItem gameInfo, string localPath, string storedBranchName, string storedBuild)
        {
            LocalPath = localPath;

            if (string.IsNullOrEmpty(LocalPath)) //Game is not install
            {
                InstallStatus = gameInfo.Ownership == GameOwnership.None 
                    ? GameInstallStatus.NotPurchased 
                    : GameInstallStatus.NotInstalled;
                State = GameState.Non;
            }
            else
            {
                InstallStatus = GameInstallStatus.Installed;

                //restore branch
                GameBranchItem storedBranchInfo = null;
                if (!string.IsNullOrEmpty(storedBranchName))
                    storedBranchInfo = gameInfo.Branches.SingleOrDefault(x => x.Name == storedBranchName);

                CurrentBranch = storedBranchInfo ?? gameInfo.Branches.First();
                State = storedBranchInfo == null || storedBranchInfo.Build != storedBuild
                    ? GameState.Expired
                    : GameState.ReadyToPlay;
            }
                
            SetGameInfo(gameInfo);
        }
        
        public async Task Install(
            string sessionId,
            IProgress<InstallProgress> progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(LocalPath))
                throw new InvalidOperationException("LocalPath is invalid");

            Directory.CreateDirectory(LocalPath);
            RemoveInvalidFiles();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (InstallStatus != GameInstallStatus.NotInstalled)
                    throw new InvalidOperationException("InstallStatus is not NotInstalled");

                var manifest = await DownloadManifest(CurrentBranch.Name, sessionId, cancellationToken);
                var expiredLocalFiles = await GetExpiredLocalFiles(manifest.Files, progress, cancellationToken);
                if (expiredLocalFiles.Count > 0)
                {
                    //Do not set State to "Expired"
                    ProgressMax = expiredLocalFiles.Sum(x => x.CompressedSize);
                    ProcessingPhase = GameProcessingPhase.Downloading;

                    await installer.DownloadAndDecompresAsync(expiredLocalFiles, CurrentBranch.RootUrl, LocalPath, progress, cancellationToken);
                }

                //Install-info updates every time, because, after switch account, install-info has been deleted, but all game files is actual.
                Utils.LoadLibrary().Add(new GameInstallData
                {
                    BranchName = CurrentBranch.Name,
                    BranchBuild = CurrentBranch.Build,
                    BranchExePath = CurrentBranch.ExePath,
                    GameDirectory = LocalPath,
                    GameGuid = gameInfo.Guid
                });

                InstallStatus = GameInstallStatus.Installed;
                State = GameState.ReadyToPlay;

                Utils.LoadInterruptedProcess().Remove(Guid);
                ProcessingPhase = GameProcessingPhase.Idleness;
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                if (!(baseEx is WebException || baseEx is SocketException))
                    ProcessingPhase = GameProcessingPhase.Idleness;

                throw;
            }
        }

        public async Task Update(
            string sessionId,
            IProgress<InstallProgress> progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(LocalPath))
                throw new InvalidOperationException("LocalPath is invalid");

            try
            {
                var manifest = await DownloadManifest(CurrentBranch.Name, sessionId, cancellationToken);
                if (manifest == null)
                {
                    State = GameState.InvalidPermission;
                    return;
                }

                var expiredLocalFiles = await GetExpiredLocalFiles(manifest.Files, progress, cancellationToken);
                if (expiredLocalFiles == null || expiredLocalFiles.Count == 0)
                {
                    SetSuccessUpdateSettings();
                    return;
                }

                State = GameState.Expired;
                ProgressMax = expiredLocalFiles.Sum(x => x.CompressedSize);
                ProcessingPhase = GameProcessingPhase.Downloading;
                await installer.DownloadAndDecompresAsync(expiredLocalFiles, CurrentBranch.RootUrl, LocalPath, progress, cancellationToken);

                SetSuccessUpdateSettings();
                ProcessingPhase = GameProcessingPhase.Idleness;
            }
            catch(Exception ex)
            {
                if (!(ex is SocketException || ex is WebException))
                    ProcessingPhase = GameProcessingPhase.Idleness;

                throw;
            }
        }

        private void SetSuccessUpdateSettings()
        {
            Utils.LoadLibrary().Update(
                    gameInfo.Guid,
                    CurrentBranch.Name,
                    CurrentBranch.Build,
                    CurrentBranch.ExePath);
            State = GameState.ReadyToPlay;
        }

        public async Task Uninstall()
        {
            if (InstallStatus == GameInstallStatus.NotInstalled ||
                InstallStatus == GameInstallStatus.NotPurchased)
                throw new InvalidOperationException($"Can not uninstall game. Invalid game state : {InstallStatus}");

            await TryRemoveGameFilesAsync();

            Utils.LoadLibrary().RemoveGame(gameInfo.Guid);
            LocalPath = string.Empty;
            ProcessingPhase = GameProcessingPhase.Idleness;
            InstallStatus = GameInstallStatus.NotInstalled;
            State = GameState.Non;
        }

        public async Task SwitchBranch(
            string sessionId,
            IProgress<InstallProgress> progress,
            CancellationToken cancellationToken,
            string targetBranchName)
        {
            var targetBranch = gameInfo.Branches.SingleOrDefault(x => x.Name == targetBranchName);
            if (targetBranch == null || CurrentBranch.Name == targetBranch.Name)
                return;

            CurrentBranch = targetBranch;

            if (InstallStatus == GameInstallStatus.Installed)
            {
                Utils.LoadLibrary().Update(
                    gameInfo.Guid,
                    CurrentBranch.Name,
                    CurrentBranch.Build,
                    CurrentBranch.ExePath
                );
                State = GameState.Expired;
                await Update(sessionId, progress, cancellationToken);
            }
        }

        public async Task Purchase(string sessionId)
        {
            var gamesInfo = await HeliosApi.Provider.DownloadGameInfoList(sessionId);
            var currentGameInfo = gamesInfo.Games.Single(x => x.Guid == Guid);
            SetGameInfo(currentGameInfo);
            InstallStatus = GameInstallStatus.NotInstalled;
        }

        public void Launch()
        {
            if (State == GameState.ReadyToPlay && 
                InstallStatus == GameInstallStatus.Installed && 
                ProcessingPhase == GameProcessingPhase.Idleness)
                try
                {
                    var exePath = Path.Combine(LocalPath, CurrentBranch.ExePath);
                    var startInfo = new ProcessStartInfo(exePath);
                    startInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                    Process.Start(startInfo);
                    State = GameState.Launched;
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
        }       

        public async Task TryRemoveGameFilesAsync()
        {
            if (string.IsNullOrWhiteSpace(LocalPath))
                throw new InvalidOperationException($"Can not uninstall game. Game path is empty");

            ProcessingPhase = GameProcessingPhase.Uninstalling;
            await Task.Run(() =>
            {
                try
                {
                    var dir = new DirectoryInfo(LocalPath);
                    dir.Delete(true);
                }
                catch (DirectoryNotFoundException ex)
                {
                    //TODO Show message "Toadman launcher can't delete some of the files. Please remove them manually"
                    logger.Error(ex);
                }
                finally
                {
                    ProcessingPhase = GameProcessingPhase.Idleness;
                }
            });
        }

        private void RemoveInvalidFiles()
        {
            if (!Directory.Exists(LocalPath))
                return;

            Directory.EnumerateFiles(LocalPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(Constants.InvalidDownloadExtension) || s.EndsWith(Constants.InvalidCompressedExtension) || s.EndsWith(Constants.InvalidMergeChunks))
                .ToList()
                .ForEach(x => File.Delete(x));
        }
    }
}