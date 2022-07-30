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
    [ValueConversion(typeof(string),typeof(Visibility))]
    public class VisibleIfNonNullOrEmptyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string ;

            return (string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    

}
