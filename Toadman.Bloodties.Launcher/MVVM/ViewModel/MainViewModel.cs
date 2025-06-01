using NLog;
using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.Bloodties.Launcher
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public ICommand NavigateToNewsCommand { get; set; }
        public ICommand NavigateToBuildNotesCommand { get; set; }
        public ICommand LaunchCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        public PageType ActivePageType
        {
            get { return activePageType; }
            set { SetField(ref activePageType, value); }
        }
        public string Revision { get; set; }
        public PageBaseViewModel ActivePage
        {
            get { return activePage; }
            set { SetField(ref activePage, value); }
        }
        public Locale SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                Localizer.Locale = value;
                SetField(ref selectedLanguage, value);
                
                var config = Utils.LoadConfig<Config>();
                config.Language = value;
                config.Save();
            }
        }
        public double BlindOpacity
        {
            get { return blindOpacity; }
            set { SetField(ref blindOpacity, value); }
        }

        private double blindOpacity = 0;
        private Locale selectedLanguage;
        private PageBaseViewModel activePage;
        private Dictionary<PageType, PageBaseViewModel> pages;
        private PageType activePageType;
        private Logger logger = LogManager.GetCurrentClassLogger();

        public MainViewModel()
        {
            pages = new Dictionary<PageType, PageBaseViewModel>
            {
                { PageType.Start, new PageStartViewModel() },
                { PageType.News, new PageNewsViewModel() },
                { PageType.BuildNotes, new PageBuildNotesViewModel() }
            };

            ActivePage = pages[PageType.Start];
            NavigateToNewsCommand = new RelayCommand(() => NavigateTo(PageType.News));
            NavigateToBuildNotesCommand = new RelayCommand(() => NavigateTo(PageType.BuildNotes));
            LaunchCommand = new RelayCommand(GameLaunch);
            SettingsCommand = new RelayCommand(OpenSettings);

            var locale = Utils.LoadConfig<Config>()?.Language;
            SelectedLanguage = locale > 0 ? locale.Value : Locale.En;

            Revision = Utils.GetCurrentRevision();
        }

        private void NavigateTo(PageType targer)
        {
            if (ActivePageType == targer)
            {
                BlindOpacity = 0.0;
                targer = PageType.Start;
            }
            else
                BlindOpacity = 0.5;

            ActivePage = pages[targer];
            ActivePageType = targer;
            
        }

        private void OpenSettings()
        {
            var settingsView = new SettingsView();
            settingsView.Owner = App.Current.MainWindow;
            settingsView.ShowDialog();
        }

        private void GameLaunch()
        {
            try
            {
                var launcherDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                var exeFilePath = Path.GetFullPath(Path.Combine(launcherDir, Constants.BloodtiesExeRelPath));
                    
                if (File.Exists(exeFilePath))
                {
                    var startInfo = new ProcessStartInfo(exeFilePath);
                    startInfo.WorkingDirectory = Path.GetDirectoryName(exeFilePath);
                    Process.Start(startInfo);
                    App.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show($"Path: {exeFilePath}", "Bloodties executable not found", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Process.Start("notepad");
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}