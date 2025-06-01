using System;
using System.Globalization;
using System.Windows.Data;

namespace Toadman.Bloodties.Launcher
{
    public class ScreenModeToLocaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var screenMode = (BloodtiesScreenMode)value;
            switch (screenMode)
            {
                case BloodtiesScreenMode.Windowed:
                    return Localizer.Instance.Get("display_mode_windowed");
                case BloodtiesScreenMode.FullScreen:
                    return Localizer.Instance.Get("display_mode_fullscreen");
                case BloodtiesScreenMode.Borderless:
                    return Localizer.Instance.Get("display_mode_borderless");
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