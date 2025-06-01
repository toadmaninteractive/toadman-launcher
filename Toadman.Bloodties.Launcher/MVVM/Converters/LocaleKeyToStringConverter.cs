using Protocol;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Toadman.Bloodties.Launcher
{
    public class LocaleKeyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var locale = (Locale)value;
            switch (locale)
            {
                case Locale.En:
                    return "English";
                case Locale.De:
                    return "German";
                case Locale.Fr:
                    return "French";
                case Locale.Ru:
                    return "Russian";
                case Locale.Ja:
                    return "Japanese";
                case Locale.Zh:
                    return "Chinese";
                default:
                    throw new ArgumentException("value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}