using MahApps.Metro.Controls;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : MetroWindow
    {
        public ICommand ApplyCommand { get; set; }

        public ApplicationUpdateChannel SelectedUpdateChannel { get; set; }
        public bool AutoRun { get; set; }

        public SettingsView()
        {
            InitializeComponent();

            var config = Core.Utils.LoadConfig<Config>();

            SelectedUpdateChannel = config.Channel;
            AutoRun = config.AutoRun;
            ApplyCommand = new RelayCommand(() =>
            {
                config.Channel = SelectedUpdateChannel;
                config.AutoRun = AutoRun;
                config.Save();

                DialogResult = true;
            },
            () => SelectedUpdateChannel != config.Channel || AutoRun != config.AutoRun);

            DataContext = this;
        }
    }
}
