using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for InstallationSettingsView.xaml
    /// </summary>
    public partial class InstallationSettingsView : Window
    {

        public string SelectedFolder { get; private set; }

        public InstallationSettingsView()
        {
            InitializeComponent();
        }

        private void OkHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SelectFolderHandler(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
