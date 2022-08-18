using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
[assembly:CoreAPIVersion("0.1.2")]

namespace TwoGisPlugin
{
    public class TwoGisPlugin : Plugin
    {
        public override Type ElementModelType
        {
            get
            {
                return  typeof(Model.Company);
            }
        }

        public override string Name
        {
            get
            {
                return "2gis.com scraper";
            }
        }

        public override string TargetHost
        {
            get
            {
                return "2gis.com";
            }
        }
        static string[] hosts = { "2gis.com", "2gis.ae", "2gis.au" };

        public override IEnumerable<FilterComponenetDescription> FiltersDescription
        {
            get
            {
                return new FilterComponenetDescription[]
                {
                    new FilterComponenetDescription()
                    {
                         Header="Filter Categories",
                          Type= FilterComponenetType.GroupFilter,
                          PropertyName = nameof(Model.Company.category)
                    }
                };
            }
        }

        public override bool ValidateTargetPageInputQuery(string input)
        {
            return true;
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri) == false)
            {
                return false;
            }
            string website = uri.Host.ToLower();
            if ((TargetHost != null) && !((hosts.Contains(website) ))) return false;
            return true;
        }

        public override string ElementName { get { return "Company"; } }

        public override string ElementNamePlural { get { return "Companies"; } }
       

        public override ScrapingTaskBase GetTask(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public override ScrapingTaskBase GetTask(string targetPage)
        {
            return new TwoGisScrapingTask(targetPage);
        }
    }
}
