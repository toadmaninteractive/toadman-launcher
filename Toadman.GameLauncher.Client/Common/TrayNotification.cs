using System;
using System.IO;
using System.Windows;
using Toadman.GameLauncher.Core;
using SWF = System.Windows.Forms;

namespace Toadman.GameLauncher.Client
{
    public class TrayNotification
    {
        private Config config = Core.Utils.LoadConfig<Config>();
        private SWF.NotifyIcon trayIcon;
        private SWF.MenuItem exitMenuItem;
        private SWF.MenuItem launchAtStartupMenuItem;
        // private List<string> showedItems = new List<string>();

        public TrayNotification() { }

        public void Start(Stream iconStream)
        {
            trayIcon = new SWF.NotifyIcon();

            launchAtStartupMenuItem = new SWF.MenuItem();
            launchAtStartupMenuItem.Text = "Launch at Startup";
            launchAtStartupMenuItem.Checked = config.AutoRun;
            launchAtStartupMenuItem.Click += LaunchAtStartupClick;

            exitMenuItem = new SWF.MenuItem();
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += ExitClick;

            var contextMenu = new SWF.ContextMenu();
            //contextMenu.MenuItems.Add(launchAtStartupMenuItem);
            contextMenu.MenuItems.Add(exitMenuItem);
            trayIcon.ContextMenu = contextMenu;

            trayIcon.Icon = new System.Drawing.Icon(iconStream);
            trayIcon.Visible = true;

            EventHandler handler = delegate (object sender, EventArgs args)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var application = (App)Application.Current;
                    application.RestoreFromTray();
                });
            };
            trayIcon.Click += handler;
            trayIcon.BalloonTipClicked += handler;
        }

        public void Dispose()
        {
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }

            if (launchAtStartupMenuItem != null)
                launchAtStartupMenuItem.Click -= LaunchAtStartupClick;

            if (exitMenuItem != null)
                exitMenuItem.Click -= ExitClick;
        }

        private void LaunchAtStartupClick(object sender, EventArgs e)
        {
            config.AutoRun = !config.AutoRun;
            config.Save();
            launchAtStartupMenuItem.Checked = config.AutoRun;
        }

        private void ExitClick(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Current.Shutdown();
        }
    }
}