using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        public static ResourceDictionary Languages;
        private GameLauncherApp app;
        private ApplicationUpdater appUpdater = new ApplicationUpdater();
        private TrayNotification tray;

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (Environment.OSVersion.Version.Major <= 6 && Environment.OSVersion.Version.Minor <= 1)
            {
                // Older than Windows 8
                WinAPI.SetProcessDPIAware();
            } else
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

            base.OnStartup(e);

            if (!Directory.Exists(FolderNames.AppDataFolderPath))
                Directory.CreateDirectory(FolderNames.AppDataFolderPath);

            //Init config file if not exist
            var config = Core.Utils.LoadConfig<Config>();

            ServicePointManager.DefaultConnectionLimit = Constants.DefaultConnectionLimit;
            Languages = GetLanguage();
            app = new GameLauncherApp();

            try
            {
                await CheckUpdate(config.Channel);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.NameResolutionFailure)
                    throw;
            }

            tray = new TrayNotification();
            tray.Start(Application.GetResourceStream(new Uri("Resources/Icons/Toadman.ico", UriKind.Relative)).Stream);

            app.Run(e.Args.Contains("-silent")).Track();
        }

        private ResourceDictionary GetLanguage(string cultureName = null)
        {
            var dict = new ResourceDictionary();
            switch (cultureName)
            {
                case "ru-RU":
                    dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", cultureName), UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                    break;
            }

            return dict;
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

        public void RestoreFromTray()
        {
            app.RestoreFromTray();
        }

        private async Task CheckUpdate(ApplicationUpdateChannel channel)
        {
            var currentRev = Core.Utils.GetCurrentRevision();
            var actualRev = await appUpdater.GetRemoteRevisionAsync(channel);
            if (string.IsNullOrEmpty(currentRev) || actualRev != currentRev)
            {
                if (!appUpdater.CheckForSetupFile(channel, actualRev))
                {
                    var connectView = new LoadingView();
                    connectView.DataContext = appUpdater;
                    connectView.Show();

                    await appUpdater.DownloadUpdatesAsync(channel, actualRev);
                }

                if (!string.IsNullOrWhiteSpace(appUpdater.UpdateFileName))
                {
                    Process.Start(appUpdater.UpdateFileName, "/SILENT");
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
