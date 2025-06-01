using System;
using System.Windows;
using System.Windows.Controls;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class GameInstallStatusTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NotPurchasedTemplate { get; set; }
        public DataTemplate NotInstalledTemplate { get; set; }
        public DataTemplate InstalledTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            GameViewModel gameVM = (GameViewModel)item;
            switch (gameVM.Model.InstallStatus)
            {
                case GameInstallStatus.NotPurchased:
                    return NotPurchasedTemplate;
                case GameInstallStatus.NotInstalled:
                    return NotInstalledTemplate;
                case GameInstallStatus.Installed:
                    return InstalledTemplate;
                default:
                    throw new ArgumentException();
            }
        }
    }
}