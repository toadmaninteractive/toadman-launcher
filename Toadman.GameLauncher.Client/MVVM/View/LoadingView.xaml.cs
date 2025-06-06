﻿using MahApps.Metro.Controls;
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
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for LoadingView.xaml
    /// </summary>
    public partial class LoadingView : MetroWindow
    {
        private ILoadingUpdate context => (ILoadingUpdate)DataContext;

        public LoadingView()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            context.CancelDownload();
            base.OnClosed(e);
        }
    }
}
