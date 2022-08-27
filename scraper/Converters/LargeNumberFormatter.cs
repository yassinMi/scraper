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
    /// if no param, defaults to v3.1
    /// 
    /// param = v3
    /// 1 523 000 1.52M
    /// 152 500=> 152k
    /// 11 500 => 11.5k
    /// 8 520  => 8.52k
    /// 8 520  => 8520    (param = v3.1) 
    /// 520    => 520 
    /// 
    /// param = v2
    /// 1 523 000 1.5M
    /// 152 500=> 152k   
    /// 11 500 => 11k
    /// 8 520  => 8.5k
    /// 8 520  => 8520    (param = v2.1)
    /// 520    => 520     
    /// 
    /// /// param = v1
    /// 1 523 000 1M
    /// 152 500=> 152k   
    /// 11 500 => 11k
    /// 8 520  => 8k
    /// 8 520  => 8520    (param = v1.1)
    /// 520    => 520     
    /// NOTE the default param value is zero if not specfied
    /// </summary>
    [ValueConversion(typeof(int),typeof(string))]
    public class LargeNumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? inp = (value as int?);
            if (inp.HasValue == false) return "";
            string v = (parameter as string);
            if (string.IsNullOrWhiteSpace(v)) v = "v3.1";
            float i = inp.Value;
            string res = "";
            string ci = "kMG";
            int ci_ix = -1;
            
            int threshold = v.EndsWith(".1") ? 9999 : 999;
            char G = v[1];
            if (i <= threshold)
            {
                return inp.Value.ToString();
            }
            while (i> threshold)
            {
                i = i / 1000;
                ci_ix++;
            }
            if (ci_ix > 2)//too big
            {
                return inp.Value.ToString();
            }
            return $"{(i>Math.Pow(10, G=='1'?1:G=='2'?2:3)?Math.Ceiling(i).ToString():string.Format("{0:G"+ G+"}", i))}{ci[ci_ix]}" ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    


}
