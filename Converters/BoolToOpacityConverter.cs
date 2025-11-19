using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace DogPoemApp.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public double TrueOpacity { get; set; } = 1.0;
        public double FalseOpacity { get; set; } = 0.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueOpacity : FalseOpacity;

            try
            {
                return System.Convert.ToBoolean(value) ? TrueOpacity : FalseOpacity;
            }
            catch
            {
                return FalseOpacity;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}