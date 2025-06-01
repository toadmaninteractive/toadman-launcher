using System;
using System.Globalization;
using System.Windows.Data;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class IdlenessPhaseToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GameProcessingPhase)value == GameProcessingPhase.Idleness;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}