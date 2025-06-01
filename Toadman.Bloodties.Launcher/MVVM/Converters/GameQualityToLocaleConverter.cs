using System;
using System.Globalization;
using System.Windows.Data;

namespace Toadman.Bloodties.Launcher
{
    public class GameQualityToLocaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gameQuality = (BloodtiesGameQuality)value;
            switch (gameQuality)
            {
                case BloodtiesGameQuality.Lowest:
                    return Localizer.Instance.Get("graphics_quality_lowest");
                case BloodtiesGameQuality.Low:
                    return Localizer.Instance.Get("graphics_quality_low");
                case BloodtiesGameQuality.Medium:
                    return Localizer.Instance.Get("graphics_quality_medium");
                case BloodtiesGameQuality.High:
                    return Localizer.Instance.Get("graphics_quality_high");
                case BloodtiesGameQuality.Extreme:
                    return Localizer.Instance.Get("graphics_quality_extreme");
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