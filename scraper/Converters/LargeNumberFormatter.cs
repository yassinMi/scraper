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

    /// <summary>
    /// 11 500 => 11.5k
    /// NOTE the default param value is zero if not specfied
    /// </summary>
    [ValueConversion(typeof(int),typeof(string))]
    public class LargeNumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? inp = (value as int?);
            if (inp.HasValue == false) return "";
            float i = inp.Value;
            string res = "";
            string ci = "kMG";
            int ci_ix = -1;
            if (i <= 9999)
            {
                return inp.Value.ToString();
            }
            while (i>9999)
            {
                i = i / 1000;
                ci_ix++;
            }
            if (ci_ix > 2)//too big
            {
                return inp.Value.ToString();
            }
            return $"{string.Format("{0:G3}", i)}{ci[ci_ix]}" ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    


}
