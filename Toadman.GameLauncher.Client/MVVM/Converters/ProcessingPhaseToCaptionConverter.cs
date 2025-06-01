using System;
using System.Globalization;
using System.Windows.Data;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class ProcessingPhaseToCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var phase = (GameProcessingPhase)value;
            switch (phase)
            {
                case GameProcessingPhase.FilesIntegrityChecking:
                    return "Verifying integrity of game files";
                case GameProcessingPhase.ManifestUpdating:
                    return "Updating manifest";
                case GameProcessingPhase.Downloading:
                    return "Downloading";
                case GameProcessingPhase.Decompressing:
                    return "Decompressing";
                case GameProcessingPhase.Uninstalling:
                    return "Uninstalling";
                case GameProcessingPhase.Idleness:
                    return string.Empty;
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