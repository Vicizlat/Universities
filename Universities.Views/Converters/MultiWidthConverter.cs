using System;
using System.Windows.Data;
using System.Globalization;

namespace Universities.Views.Converters
{
    public class MultiWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double mainWidth = (double)values[0];
            double result = mainWidth - 40;
            for (int i = 1; i < values.Length; i++)
            {
                result -= (double)values[i];
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}