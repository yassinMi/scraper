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
using System.Windows.Shapes;

namespace scraper.View
{
    /// <summary>
    /// Interaction logic for UnhandledErrorWindow.xaml
    /// must assign dataContext from the caller
    /// </summary>
    public partial class UnhandledErrorWindow : Window
    {
        public UnhandledErrorWindow()
        {
            InitializeComponent();
            

        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);  
            if(e.Property == UnhandledErrorWindow.DataContextProperty)
            {
                ((UnhandledErrorWindowVM)e.NewValue).WindowObj = this;
            }
        }


    }
}
