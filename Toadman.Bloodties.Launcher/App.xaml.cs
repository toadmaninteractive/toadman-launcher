using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using Toadman.GameLauncher.Core;
using Protocol;
using System.Threading.Tasks;

namespace Toadman.Bloodties.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private readonly string localizerSourceFilePath = "Toadman.Bloodties.Launcher.Localization.strings.strings.json";
        private ApplicationUpdater appUpdater = new ApplicationUpdater();

        protected async override void OnStartup(StartupEventArgs e)
        {
            if (Environment.OSVersion.Version.Major <= 6 && Environment.OSVersion.Version.Minor <= 1)
            {
                // Older than Windows 8
                WinAPI.SetProcessDPIAware();
            }
            else
            {
                // Windows 8 or newer
                WinAPI.SetProcessDpiAwareness(WinAPI.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            }

            if (!SingleInstance<App>.InitializeAsFirstInstance(Constants.AppGuid))
            {
                var launcherProcces = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == Constants.AppName);
                if (launcherProcces != null)
                    Current.Dispatcher.Invoke(() => NativeMethods.SetForegroundWindow(launcherProcces.MainWindowHandle));

                Current.Shutdown();
                SingleInstance<App>.Cleanup();
                return;
            }

            if (!Directory.Exists(FolderNames.AppDataFolderPath))
                Directory.CreateDirectory(FolderNames.AppDataFolderPath);

            //Init config file if not exist
            Utils.LoadConfig<Config>();

            base.OnStartup(e);

            // Comment out for standalone Steam build
            await CheckUpdate();

            Localizer.Instance.Load(localizerSourceFilePath);

            var mainView = new MainView();
            mainView.DataContext = new MainViewModel();
            mainView.Show();
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
            }

            return true;
        }

        private async Task CheckUpdate()
        {
            /*
            var isNetworkAvailable = await Utils.CheckForInternetConnectionSafeAsync();
            if (!isNetworkAvailable)
                return;

            var config = Utils.LoadConfig<Config>();
            var actualRev = await appUpdater.GetRemoteRevisionAsync(config.Channel);
            var currentRev = Utils.GetCurrentRevision();

            if (string.IsNullOrEmpty(currentRev) || actualRev != currentRev)
            {
                if (!appUpdater.CheckForSetupFile(config.Channel, actualRev))
                {
                    var loadingView = new LoadingView();
                    loadingView.DataContext = appUpdater;
                    loadingView.Show();
                    await appUpdater.DownloadUpdatesAsync(config.Channel, actualRev);
                }
                
                if (!string.IsNullOrEmpty(appUpdater.UpdateFileName))
                {
                    Process.Start(appUpdater.UpdateFileName, "/SILENT");
                    Application.Current.Shutdown();
                }
            }
            */
        }
    }
}
