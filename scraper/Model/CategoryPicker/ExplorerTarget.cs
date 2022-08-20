using Mi.Common;
using scraper.ViewModel.CategoryPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace scraper.Model.CategoryPicker
{
    /// <summary>
    /// a 0 level category that can be scraped (contains elements)
    /// </summary>
    public class ExplorerTarget: ViewModel.BaseViewModel,ExplorerElement
    {

        CategoryPickerVM CPVM { get; set; }
        public ExplorerTarget(ViewModel.CategoryPicker.CategoryPickerVM cpVM)
        {
            this.CPVM = cpVM;
        }
        public List<ExplorerElement> Children { get; set; }

        public string Name { get; set; }

        public ExplorerElementType Type { get { return ExplorerElementType.Target; } }

        public string URL { get; set; }
        public int ElementsCount { get; set; }


        public ICommand RequestStartCommand { get { return new MICommand(hndlRequestStartCommand); } }

        private void hndlRequestStartCommand()
        {
            CPVM?.OnStartRequested(this);
        }


    }
}
