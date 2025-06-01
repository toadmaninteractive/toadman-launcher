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
    /// Interaction logic for DriveSelectorView.xaml
    /// </summary>
    public partial class DriveSelectorView : MetroWindow
    {
        public DriveSelectorView()
        {
            InitializeComponent();
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
