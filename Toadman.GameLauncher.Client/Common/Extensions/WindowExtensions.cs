using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Toadman.GameLauncher.Client
{
    public static class WindowExtensions
    {
        public static void SetDialogResult(MetroWindow radWindow, bool? value)
        {
            if (radWindow != null)
            {
                radWindow.DialogResult = value;
                if (value != null) radWindow.Close();
            }
        }

        public static bool? GetDialogResult(MetroWindow radWindow)
        {
            if (radWindow != null)
                return radWindow.DialogResult;
            
            return null;
        }

        public static readonly DependencyProperty BindableDialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof(bool?), typeof(WindowExtensions), new PropertyMetadata(BindableDialogResultPropertyChanged));

        private static void BindableDialogResultPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetDialogResult(d as MetroWindow, e.NewValue as bool?);
        }
    }
} 