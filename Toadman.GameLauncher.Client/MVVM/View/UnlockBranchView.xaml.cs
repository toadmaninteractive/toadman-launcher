using MahApps.Metro.Controls;
using System.Windows;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for UnlockBranchView.xaml
    /// </summary>
    public partial class UnlockBranchView : MetroWindow
    {
        private string gameGuid;

        public UnlockBranchView(string gameGuid)
        {
            InitializeComponent();
            this.gameGuid = gameGuid;
        }

        private async void OkHandler(object sender, RoutedEventArgs e)
        {
            LockGUI(true);
            var response
                = await HeliosApi.Provider.UnlockGameBranch(
                    txtUnlockBranchPassword.Text, gameGuid, Core.Utils.LoadConfig<Config>().SessionId);
            LockGUI(false);

            if (response.Result)
                DialogResult = true;
            else
                txtError.Text = (string)App.Languages["error_UnlockBranch_InvalidPassword"];
        }

        private void LockGUI(bool isLocked)
        {
            crlProgressRing.IsActive = isLocked;
            spContent.IsEnabled = !isLocked;
            spContent.Opacity = isLocked ? 0.25 : 1;
        }
    }
}
