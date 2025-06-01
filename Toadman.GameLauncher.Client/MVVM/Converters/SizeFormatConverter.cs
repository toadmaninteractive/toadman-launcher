using System;
using System.Globalization;
using System.Windows.Data;

namespace Toadman.GameLauncher.Client
{
    public class SizeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertToStr(value);
        }

        public string ConvertToStr(object value)
        {
            if (value == null)
                return "0";

            long size = (long)value;
            if (size < 1024L)
            {
                return string.Format("{0} bytes", size);
            }
            if (size < 1048576L)
            {
                return string.Format("{0} kB", ((float)size / 1024f).ToString("F"));
            }
            if (size < 1073741824L)
            {
                return string.Format("{0} MB", ((float)size / 1048576f).ToString("F"));
            }
            return string.Format("{0} GB", ((float)size / 1.07374182E+09f).ToString("F"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}