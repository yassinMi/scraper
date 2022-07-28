using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{


    public class TargetPageUrlUseCaseHelp
    {
        public IEnumerable<string> ExampleUrls { get; set; }
        public string Description { get; set; }
    }
    public class PluginUsageInfo
    {
        public PluginUsageInfo()
        {
            UsageInfoViewHeader = "Supported URL's:";
            UseCases = new TargetPageUrlUseCaseHelp[] {
                    new TargetPageUrlUseCaseHelp() {Description="You can scrap category pages", ExampleUrls= new string[] { "https://www.businesslist.ph/cat/manila/3", "https://www.businesslist.ph/cat/manila/2" } },
                    new TargetPageUrlUseCaseHelp() {Description="You can scrap locations pages",

            ExampleUrls = new string[] {
                     "https://www.businesslist.ph/location/manila/3",
                        "https://www.businesslist.ph/location/manila/2" }},
                };
        }
        public string UsageInfoViewHeader { get; set; }
        public IEnumerable<TargetPageUrlUseCaseHelp> UseCases { get; set; }
    }

}
