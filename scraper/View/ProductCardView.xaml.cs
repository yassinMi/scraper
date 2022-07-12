using scraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace scraper.View
{
    /// <summary>
    /// Interaction logic for ProductCardView.xaml
    /// </summary>
    public partial class ProductCardView : UserControl
    {
        public ProductCardView()
        {
            InitializeComponent();
        }

        private void sku_val2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pvm = ((ProductViewModel)DataContext);
            if (pvm != null)
            {
                pvm.CopyToClipCommand.Execute("sku");
            }
        }
    }
}
