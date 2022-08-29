using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using BayutPlugin.Model;
[assembly:CoreAPIVersion("0.1.3")]
namespace BayutPlugin
{
    public class BayutPlugin : Plugin
    {
        public override Type ElementModelType { get { return typeof(BProperty); } }
        public override string Name { get { return "bayut scraper"; } } //todo replace plugin name
        public override string TargetHost { get { return "www.bayut.com"; } } //todo replace host name
        public override ScrapingTaskBase GetTask(TaskInfo taskInfo)
        {
            return new BayutScrapingTask(taskInfo.OriginalURL);
        }
        public override string ElementName { get { return "Property"; } } //todo replace ElementName name
        public override string ElementNamePlural { get { return "Properties"; } }
        public override Version Version { get { return new Version(1, 0, 1); } }
        public override ScrapingTaskBase GetTask(string targetPage)
        {
            return new BayutScrapingTask(targetPage);
        }
        public override PluginUsageInfo UsageInfo
        {
            get
            {
                return new PluginUsageInfo()
                {
                    UsageInfoViewHeader = "Supported URL's:",
                    UseCases = new TargetPageUrlUseCaseHelp[]{
                          new TargetPageUrlUseCaseHelp()
                          {
                               Description = "Properties listing pages",
                               ExampleUrls = new string[]
                               {
                                   "https://www.bayut.com/to-rent/property/dubai",
                                   "https://www.bayut.com/to-rent/property/dubai/page-5"
                               }
                          }
                      }
                };
            }
        }
        public override bool ValidateTargetPageInputQuery(string input)
        {
            return base.ValidateTargetPageInputQuery(input);

        }
    }
}
