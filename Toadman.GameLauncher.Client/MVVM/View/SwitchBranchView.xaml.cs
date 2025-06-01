using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for SwitchBranchView.xaml
    /// </summary>
    public partial class SwitchBranchView : MetroWindow
    {
        public string SelectedBranchName;

        private string originBranchName;

        public SwitchBranchView(string[] availableBranchNames, string currentBranchName)
        {
            InitializeComponent();

            originBranchName = currentBranchName;

            foreach (var availableBranchName in availableBranchNames)
                cbxAvailableBranchNames.Items.Add(availableBranchName);

            cbxAvailableBranchNames.SelectedItem = currentBranchName;
        }

        private void OkHandler(object sender, RoutedEventArgs e)
        {
            SelectedBranchName = (string)cbxAvailableBranchNames.SelectedItem;
            DialogResult = true;
        }

        private void SelectionChangedHandel(object sender, SelectionChangedEventArgs e)
        {
            btnSwitchBranch.IsEnabled = (string)cbxAvailableBranchNames.SelectedItem != originBranchName;
        }
    }
}
