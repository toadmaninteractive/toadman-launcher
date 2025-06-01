using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.Bloodties.Launcher
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : MetroWindow
    {
        public ICommand ApplyCommand { get; set; }

        public List<VideoMode> AvailableResolutions { get; set; }


        public BloodtiesScreenMode SelectedScreenMode { get; set; }
        public BloodtiesGameQuality SelectedQualityMode { get; set; }
        public VideoMode SelectedResolution { get; set; }

        private BloodtiesSettings bloodtiesConfig = new BloodtiesSettings();

        public SettingsView()
        {
            InitializeComponent();

            var bloodtiesConfig = new BloodtiesSettings();

            AvailableResolutions = bloodtiesConfig.AvailableVideoModes;
            
            SelectedScreenMode = bloodtiesConfig.ScreenMode;
            SelectedQualityMode = bloodtiesConfig.GameQuality;
            SelectedResolution = bloodtiesConfig.GameVideoMode;

            ApplyCommand = new RelayCommand(Apply);

            DataContext = this;
        }

        private void Apply()
        {
            bloodtiesConfig.GameVideoMode = SelectedResolution;
            bloodtiesConfig.GameQuality = SelectedQualityMode;
            bloodtiesConfig.ScreenMode = SelectedScreenMode;
            bloodtiesConfig.Save();

            DialogResult = true;
        }
    }
}
