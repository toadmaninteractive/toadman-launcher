using System;
using System.Globalization;
using System.Windows.Data;

namespace Toadman.Bloodties.Launcher
{
    public class LocaleKeyToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var targetSource = (string)parameter;
            return Localizer.Instance.Get(targetSource);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
