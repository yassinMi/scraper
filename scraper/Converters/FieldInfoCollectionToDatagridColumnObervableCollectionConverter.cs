using scraper.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace scraper.Converters
{
    [ValueConversion(typeof(IEnumerable<Field>), typeof(ObservableCollection<DataGridColumn>))]
    class FieldInfoCollectionToDatagridColumnObervableCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<Field> fields = value as IEnumerable<Field>;
            if (fields == null) return null;
            ObservableCollection<DataGridColumn> res = new ObservableCollection<DataGridColumn>();
            foreach (var f in fields)
            {
                DataGridColumn c = new DataGridTextColumn();
                c.Header = f.UIName;
                res.Add(c);
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
