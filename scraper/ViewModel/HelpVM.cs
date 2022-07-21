using scraper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.ViewModel
{
   
    public class HelpVM : BaseViewModel
    {
        public HelpVM()
        {
            var model = new PluginUsageInfo();
            Header = model.UsageInfoViewHeader;
            UseCases = model.UseCases;
            notif(nameof(Header));
            notif(nameof(UseCases));
        }
        public HelpVM( PluginUsageInfo pui)
        {
            Header = pui.UsageInfoViewHeader;
            UseCases = pui.UseCases;
            notif(nameof(Header));
            notif(nameof(UseCases)); 
        }

        public string Header { get; set; }
        public IEnumerable<TargetPageUrlUseCaseHelp> UseCases {get;set;} 
    }
}
