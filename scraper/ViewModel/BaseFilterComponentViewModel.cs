using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;

namespace scraper.ViewModel
{
    public class BaseFilterComponentViewModel : BaseViewModel
    {
        public FilterComponentBase Model { get; set; }
        
        private string _Header;
        public virtual string Header
        {
            set { _Header = value; notif(nameof(Header)); }
            get { return _Header; }
        }


    }
}
