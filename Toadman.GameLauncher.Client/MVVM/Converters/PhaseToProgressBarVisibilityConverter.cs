using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class PhaseToProgressBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var phase = (GameProcessingPhase)value;
            switch (phase)
            {
                case GameProcessingPhase.FilesIntegrityChecking:
                case GameProcessingPhase.ManifestUpdating:
                case GameProcessingPhase.Downloading:
                case GameProcessingPhase.Decompressing:
                case GameProcessingPhase.Uninstalling:
                    return Visibility.Visible;
                case GameProcessingPhase.Idleness:
                    return Visibility.Collapsed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}