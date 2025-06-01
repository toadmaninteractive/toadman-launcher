using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public partial class GameLibraryViewModel : NotifyPropertyChanged
    {
        public event Action SignInChange;

        public ICommand AppUpdateCommand { get; set; }
        public ICommand GoToDiscordCommand { get; set; }

        public string Title { get; set; }
        public string CurrentUserName
        {
            get { return currentUserName; }
            set { SetField(ref currentUserName, value); }
        }
        public ObservableCollection<GameViewModel> Games { get; set; }
            = new ObservableCollection<GameViewModel>();
        public ObservableCollection<ErrorMessage> Errors { get; set; }
            = new ObservableCollection<ErrorMessage>();
        public string BackgroundImageSource
        {
            get { return backgroundImageSource; }
            set { SetField(ref backgroundImageSource, value); }
        }
        public string PreveiewBackgroundImageSource
        {
            get { return preveiewBackgroundImageSource; }
            set { SetField(ref preveiewBackgroundImageSource, value); }
        }
        public Stretch BackgroundStretch
        {
            get { return backgroundStretch; }
            set { SetField(ref backgroundStretch, value); }
        }
        public GameViewModel SelectedGame
        {
            get { return selectedGame; }
            set
            {
                SetField(ref selectedGame, value);
                if (selectedGame == null)
                {
                    BackgroundStretch = Stretch.None;
                    BackgroundImageSource = backgroundDefaultImageSource;
                }
                else
                {
                    BackgroundStretch = Stretch.UniformToFill;
                    PreveiewBackgroundImageSource = BackgroundImageSource;
                    BackgroundImageSource = selectedGame.BackgroundImageSource;
                }
            }
        }
        public bool IsWaiting
        {
            get { return isWaiting; }
            set { SetField(ref isWaiting, value); }
        } public bool IsGameLoad
        {
            get { return isGameLoad; }
            set { SetField(ref isGameLoad, value); }
        }
        public bool HasUpdate
        {
            get { return hasUpdate; }
            set { SetField(ref hasUpdate, value); }
        }
        public List<System.Windows.Controls.MenuItem> AdditionalActions { get; set; }

        public string AppUpdateFileName;

        private GameViewModel selectedGame;
        private string backgroundImageSource;
        private string preveiewBackgroundImageSource;
        private bool isWaiting = false;
        private bool isGameLoad = false;
        private string currentUserName;
        private bool hasUpdate;
        private Stretch backgroundStretch = Stretch.None;
        private string backgroundDefaultImageSource = "../../Resources/MainLogo.png";
        private Logger logger = LogManager.GetCurrentClassLogger();

        public GameLibraryViewModel()
        {
            var currentRevision = Core.Utils.GetCurrentRevision();
            Title = $"Toadman Launcher Rev. #{currentRevision}";
            backgroundImageSource = backgroundDefaultImageSource;
            preveiewBackgroundImageSource = backgroundDefaultImageSource;

            AppUpdateCommand = new RelayCommand(() =>
            {
                System.Diagnostics.Process.Start(AppUpdateFileName, "/SILENT");
                Application.Current.Shutdown();
            });
            GoToDiscordCommand = new RelayCommand(() => 
            {
                if (SelectedGame?.Model?.DiscordUrl != null)
                    System.Diagnostics.Process.Start(SelectedGame.Model.DiscordUrl);
            });

            FillAdditionalActions();
        }

        public async Task LoadGames(string userName, string sessionId)
        {
            IsWaiting = true;
            CurrentUserName = userName;
            
            try
            {
                var gameInfoList = await Manifest.Instance.DownloadAndSaveAppManifest(sessionId);
                if (gameInfoList == null)
                    gameInfoList = Manifest.Instance.LoadFromFile();

                GameListUpdate(gameInfoList?.Games ?? new List<GameItem>());
                if (Games.Count > 0)
                {
                    IsGameLoad = true;
                    RestoreInstallProcesses();
                }
            }
            finally
            {
                IsWaiting = false;
            }
        }

        public void ClearError(ErrorType disconnect)
        {
            Errors.Clear();
        }
       
        private void FillAdditionalActions()
        {
            AdditionalActions = new List<System.Windows.Controls.MenuItem>();

            var logOut = Utils.GetMenuItem((string)App.Languages["header_LogOut"]);
            logOut.Command = new AsyncCommand(async () =>
            {
                CurrentUserName = string.Empty;
                var conf = Core.Utils.LoadConfig<Config>();
                await HeliosApi.Provider.Logout(conf.SessionId);
                conf.SessionOut();

                SignInChange?.Invoke();
            });
            var changePassword = Utils.GetMenuItem((string)App.Languages["header_ChangePassword"]);
            changePassword.Command = new RelayCommand(() =>
            {
                var authorizationView = new AuthorizationView(AuthorizationScreen.ChangePassword);
                authorizationView.Owner = App.Current.MainWindow;
                authorizationView.ShowInTaskbar = false;

                App.Current.MainWindow.Hide();
                if (authorizationView.ShowDialog() ?? false)
                    SignInChange?.Invoke();
                else
                    App.Current.MainWindow.Show();
            });

            var options = Utils.GetMenuItem((string)App.Languages["mainMenu_options"]);
            options.Command = new RelayCommand(() => 
            {
                var settingsView = new SettingsView();
                settingsView.ShowInTaskbar = false;
                settingsView.Owner = App.Current.MainWindow;
                settingsView.ShowDialog();
            }, () => !IsWaiting);
            
            AdditionalActions.Add(changePassword);
            AdditionalActions.Add(options);
            AdditionalActions.Add(logOut);
        }   
    }
}