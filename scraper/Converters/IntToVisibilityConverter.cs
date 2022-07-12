using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    //copied from the deeted WatcherUc 's code behind
    /// <summary>
    /// returns visible if int isstrictly greater than param, collepsed otherwise
    /// NOTE the default param value is zero if not specfied
    /// </summary>
    [ValueConversion(typeof(int),typeof(Visibility))]
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int threshold = 0;
            if (parameter != null)
                threshold = (int)parameter;
            return ((int)value > ((int)(threshold)) ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// return collapsed if boolean value is true or if int value is strictly greater than 0
    /// NOTE other types will result in visible regardless of the value
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityInverted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.GetType() == typeof(bool))
            {
                return ((bool)value ? Visibility.Collapsed : Visibility.Visible);
            }
            else if(value.GetType() == typeof(int))
            {
                return ((int)value >0? Visibility.Collapsed : Visibility.Visible);
            }
            else
            {
                return Visibility.Visible;
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
