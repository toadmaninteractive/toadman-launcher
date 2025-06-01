using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class GameViewModel : NotifyPropertyChanged
    {
        public event Action<IGameModel> OnInstallStatusChanged;

        public ICommand LaunchCommand { get; private set; }
        public ICommand InstallCommand { get; set; }
        public ICommand UninstallCommand { get; set; }
        public ICommand PurchaseCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand CancelAndRemoveCommand { get; set; }
        public ICommand ScanAndRepairCommand { get; set; }
        public ICommand SwitchBranchCommand { get; set; }
        public ICommand UnlockBranchCommand { get; set; }

        public IProgress<InstallProgress> Progress;
        public ObservableCollection<System.Windows.Controls.MenuItem> OptionActions { get; set; } 
            = new ObservableCollection<System.Windows.Controls.MenuItem>();
        public IGameModel Model { get; set; }

        public string IconMenuImageSource { get; set; }
        public string BackgroundImageSource { get; set; }
        public long ByteProgress
        {
            get { return byteProgress; }
            set { SetField(ref byteProgress, value); }
        }
        public long ByteProgressMax
        {
            get { return byteProgressMax; }
            set { SetField(ref byteProgressMax, value); }
        }
        public string EstimatedTime
        {
            get { return estimatedTime; }
            set { SetField(ref estimatedTime, value); }
        }
        public bool HasUpdate
        {
            get { return hasUpdate; }
            set
            {
                SetField(ref hasUpdate, value);
                Model.State = value ? GameState.Expired : GameState.ReadyToPlay;
            }
        }
        public string HeaderPlay
        {
            get { return headerPlay; }
            set { SetField(ref headerPlay, value); }
        }
        public Visibility SwitchBranchVisibility
        {
            get { return switchBranchVisibility; }
            set { SetField(ref switchBranchVisibility, value); }
        }
     
        private long processedSize = 0;
        private Dictionary<string, long> partiallyProcessed = new Dictionary<string, long>();
        private long byteProgress;
        private long byteProgressMax;
        private string estimatedTime;
        private bool hasUpdate;
        private string headerPlay;
        private bool isOperationComplete = true;
        private Visibility switchBranchVisibility;
        private EstimatedTimer estimatedTimer;
        private CancellationTokenSource cancellationTS;
        private Logger logger = LogManager.GetCurrentClassLogger();

        public GameViewModel(
            IGameModel model, 
            string iconMenuImage, 
            string backgroundImage)
        {
            Model = model;
            SwitchBranchVisibility = Model.AvailableBranchNames.Length > 1
                ? Visibility.Visible
                : Visibility.Collapsed;

            HeaderPlay = (string)App.Languages["header_Play"];

            model.PhaseChange += OnPhaseChange;
            model.InstallStatusChange += OnInstallStatusChange;
            model.GameStateChange += OnGameStateChange;

            IconMenuImageSource = iconMenuImage;
            BackgroundImageSource = backgroundImage;

            Progress = new Progress<InstallProgress>(ProgressReport);

            PurchaseCommand = new AsyncCommand(Purchase, () =>
                Model.InstallStatus == GameInstallStatus.NotPurchased &&
                Model.State == GameState.Non);
            InstallCommand = new AsyncCommand(InstallCommnadHandler, () => 
                Model.InstallStatus == GameInstallStatus.NotInstalled &&
                Model.ProcessingPhase == GameProcessingPhase.Idleness &&
                Model.State == GameState.Non && 
                isOperationComplete);
            UninstallCommand = new AsyncCommand(Uninstall, () =>
                Model.InstallStatus == GameInstallStatus.Installed &&
                Model.ProcessingPhase == GameProcessingPhase.Idleness);
            CancelCommand = new RelayCommand(() => cancellationTS?.Cancel(), () => Model.ProcessingPhase != GameProcessingPhase.Idleness);
            ScanAndRepairCommand = new AsyncCommand(() => Update(), () =>
                Model.InstallStatus == GameInstallStatus.Installed &&
                Model.ProcessingPhase == GameProcessingPhase.Idleness);
            SwitchBranchCommand = new RelayCommand(OpenSwitchBranch, 
                () =>Model.ProcessingPhase == GameProcessingPhase.Idleness);
            UnlockBranchCommand = new AsyncCommand(async () =>
                {
                    var view = new UnlockBranchView(Model.Guid);
                    view.Owner = App.Current.MainWindow;
                    if (view.ShowDialog() ?? false)
                    {
                        var sessionId = Core.Utils.LoadConfig<Config>()?.SessionId;
                        var gamesInfo = await HeliosApi.Provider.DownloadGameInfoList(sessionId);
                        var currentGameInfo = gamesInfo.Games.Single(x => x.Guid == Model.Guid);
                        Model.SetGameInfo(currentGameInfo);
                        SwitchBranchVisibility = Model.AvailableBranchNames.Length > 1
                            ? Visibility.Visible
                            : Visibility.Collapsed;

                        if (Model.InstallStatus == GameInstallStatus.Installed)
                            OpenSwitchBranch();
                    }
                }, () => Model.ProcessingPhase == GameProcessingPhase.Idleness);


            LaunchCommand = new AsyncCommand(UpdateAndLaunch, () =>
                Model.InstallStatus == GameInstallStatus.Installed &&
                Model.ProcessingPhase == GameProcessingPhase.Idleness &&
                (Model.State == GameState.ReadyToPlay || Model.State == GameState.Expired || Model.State == GameState.Launched) &&
                isOperationComplete);

            OnInstallStatusChange(Model.InstallStatus);
        }

        public bool EqualModel(IGameModel sender)
        {
            return sender == Model;
        }

        public async Task Purchase()
        {
            PurchaseView purchaseView = new PurchaseView(Model.Guid);
            purchaseView.Owner = App.Current.MainWindow;

            if (purchaseView.ShowDialog() ?? false)
            {
                var sessionId = Core.Utils.LoadConfig<Config>()?.SessionId;
                await Model.Purchase(sessionId);
                if (Model.InstallStatus != GameInstallStatus.NotPurchased)
                    OnInstallStatusChanged?.Invoke(Model);
            }
        }

        public async Task RestoreInstall(string localPath)
        {
            Model.LocalPath = localPath;
            await Install();
        }

        private async Task InstallCommnadHandler()
        {
            var driveSelectorView = new DriveSelectorView();
            var driveSelectorDataContext = new DriveSelectorViewModel(Model.Guid, Model.Title, Model.InstallationSize);
            driveSelectorView.DataContext = driveSelectorDataContext;
            driveSelectorView.Owner = App.Current.MainWindow;
            driveSelectorView.ShowInTaskbar = false;

            if (!(driveSelectorView.ShowDialog() ?? false))
                return;

            Model.LocalPath = driveSelectorDataContext.SelectedFolder;
            Core.Utils.LoadInterruptedProcess().Add(Model.Guid, Model.LocalPath);
            await Install();
        }

        private async Task Install()
        {
            using (cancellationTS = new CancellationTokenSource())
            {
                isOperationComplete = false;
                estimatedTimer = new EstimatedTimer();
                var sessionId = Core.Utils.LoadConfig<Config>()?.SessionId;
                var state = OperationResult.UnexpectedException;

                do
                {
                    try
                    {
                        await Model.Install(sessionId, Progress, cancellationTS.Token);
                        state = OperationResult.Success;
                    }
                    catch (OperationCanceledException)
                    {
                        state = OperationResult.Cancellation;
                        break;
                    }
                    catch (Exception ex)
                    {
                        state = OperationResult.Reconnect;
                        if (ex.GetBaseException() is SocketException) { }
                        else if (ex.GetBaseException() is WebException) { }
                        else
                        {
                            logger.Error(ex);
                            state = OperationResult.UnexpectedException;
                            break;
                        }
                    }

                    if (state == OperationResult.Reconnect)
                        await Task.Delay(Constants.ReconnectDelay);
                } while (state != OperationResult.Success);

                if (state == OperationResult.Cancellation)
                {
                    Core.Utils.LoadInterruptedProcess().Remove(Model.Guid);
                    await Model.TryRemoveGameFilesAsync();
                }

                isOperationComplete = true;

                if (Model.InstallStatus != GameInstallStatus.NotInstalled)
                    OnInstallStatusChanged?.Invoke(Model);//Do not touch

                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        private async Task UpdateAndLaunch()
        {
            try
            {
                var sessionId = Core.Utils.LoadConfig<Config>().SessionId;
                var actualGamesInfo = await Manifest.Instance.DownloadAndSaveAppManifest(sessionId);
                if (actualGamesInfo != null)
                {
                    var actualGameInfo = actualGamesInfo.Games.SingleOrDefault(x => x.Guid == Model.Guid);
                    if (actualGameInfo != null)
                        Model.SetGameInfo(actualGameInfo);

                    var expiredGameGuids = Utils.GetExpiredGameGuids(actualGamesInfo);
                    HasUpdate = expiredGameGuids.Contains(Model.Guid);
                }
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                if (baseEx is SocketException) { }
                else if (baseEx is WebException) { }
                else
                    logger.Error(ex);
            }
            
            if (Model.State == GameState.Expired
                || Model.State == GameState.Launched)
                await Update(false);

            Model.Launch();
        }

        private async Task Update(bool reconnect = true)
        {
            using (cancellationTS = new CancellationTokenSource())
            {
                isOperationComplete = false;
                estimatedTimer = new EstimatedTimer();
                var sessionId = Core.Utils.LoadConfig<Config>()?.SessionId;
                var state = OperationResult.UnexpectedException;

                do
                {
                    try
                    {
                        await Model.Update(sessionId, Progress, cancellationTS.Token);
                        state = OperationResult.Success;
                    }
                    catch (OperationCanceledException)
                    {
                        state = OperationResult.Cancellation;
                        break;
                    }
                    catch (Exception ex)
                    {
                        state = OperationResult.Reconnect;
                        if (ex.GetBaseException() is SocketException) { }
                        else if (ex.GetBaseException() is WebException) { }
                        else
                        {
                            logger.Error(ex);
                            state = OperationResult.UnexpectedException;
                            break;
                        }
                    }

                    if (state == OperationResult.Reconnect)
                        if (reconnect)
                            await Task.Delay(Constants.ReconnectDelay);
                        else
                            break;
                } while (state != OperationResult.Success);

                isOperationComplete = true;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task Uninstall()
        {
            MessageBoxResult messageBoxResult =
                System.Windows.MessageBox.Show($"Are you sure you want to uninstall {Model.Title}?", "Uninstall confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            await Model.Uninstall();
            if (Model.InstallStatus != GameInstallStatus.Installed)
                OnInstallStatusChanged?.Invoke(Model); //DO not touch
        }
      
        private void ProgressReport(InstallProgress currentProgress)
        {
            switch (currentProgress.Stage)
            {
                case FileProcessingStage.Processing:
                    partiallyProcessed[currentProgress.Url] = currentProgress.BytesProcessed;
                    break;
                case FileProcessingStage.Complete:
                    processedSize += currentProgress.BytesTotal;
                    partiallyProcessed.Remove(currentProgress.Url);
                    break;
                default:
                    break;
            }

            ByteProgress = processedSize + partiallyProcessed.Values.Sum();

            var eta = estimatedTimer.GetETA(ByteProgress, Model.ProgressMax);
            if (eta.HasValue)
                EstimatedTime = eta == 0
                    ? string.Empty
                    : $"  (ETA ~ {TimeSpan.FromMilliseconds(eta.Value).ToString("hh\\:mm\\:ss")})";
        }

        private void OnPhaseChange(GameProcessingPhase phase)
        {
            processedSize = 0;
            partiallyProcessed.Clear();

            switch (phase)
            {
                case GameProcessingPhase.FilesIntegrityChecking:
                case GameProcessingPhase.Downloading:
                case GameProcessingPhase.Decompressing:
                    ByteProgress = 0;
                    ByteProgressMax = Model.ProgressMax;
                    break;
                case GameProcessingPhase.ManifestUpdating:
                case GameProcessingPhase.Uninstalling:
                    ByteProgressMax = 1;
                    break;
                case GameProcessingPhase.Idleness:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Non:
                case GameState.InvalidPermission:
                case GameState.Launched:
                    break;
                case GameState.Expired:
                    HeaderPlay = (string)App.Languages["header_UpdateAndPlay"];
                    break;
                case GameState.ReadyToPlay:
                    HeaderPlay = (string)App.Languages["header_Play"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnInstallStatusChange(GameInstallStatus installStatus)
        {
            OptionActions.Clear();
            switch (installStatus)
            {
                case GameInstallStatus.NotPurchased:
                    break;
                case GameInstallStatus.NotInstalled:
                    var unlockBranchNotInstalled = Utils.GetMenuItem((string)App.Languages["header_UnlockBranch"]);
                    unlockBranchNotInstalled.Command = UnlockBranchCommand;
                    OptionActions.Add(unlockBranchNotInstalled);
                    break;
                case GameInstallStatus.Installed:
                    var unlockBranch = Utils.GetMenuItem((string)App.Languages["header_UnlockBranch"]);
                    unlockBranch.Command = UnlockBranchCommand;
                    OptionActions.Add(unlockBranch);

                    var scanAndRepair = Utils.GetMenuItem((string)App.Languages["header_ScanAndRepair"]);
                    scanAndRepair.Command = ScanAndRepairCommand;
                    OptionActions.Add(scanAndRepair);

                    var uninstall = Utils.GetMenuItem((string)App.Languages["header_Uninstall"]);
                    uninstall.Command = UninstallCommand;
                    OptionActions.Add(uninstall);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void OpenSwitchBranch()
        {
            var view = new SwitchBranchView(Model.AvailableBranchNames, Model.CurrentBranchName);
            view.Owner = App.Current.MainWindow;
            if (view.ShowDialog() ?? false)
                SwitchBranch(view.SelectedBranchName).Track();
        }

        private async Task SwitchBranch(string targetBranchName)
        {
            using (cancellationTS = new CancellationTokenSource())
            {
                estimatedTimer = new EstimatedTimer();
                var sessionId = Core.Utils.LoadConfig<Config>()?.SessionId;
                await Model.SwitchBranch(sessionId, Progress, cancellationTS.Token, targetBranchName);
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}