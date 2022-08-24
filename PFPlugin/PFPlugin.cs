using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using PFPlugin.Model;
[assembly:CoreAPIVersion("0.1.2")]
namespace PFPlugin
{
    public class PFPlugin : Plugin
    {
        public override Type ElementModelType { get { return typeof(Agent); } }

        public override string Name { get { return "propertyfinder scraper"; } }

        public override string TargetHost { get { return "www.propertyfinder.ae"; } }
        public override ScrapingTaskBase GetTask(TaskInfo taskInfo)
        {
            return new PFScrapingTask(taskInfo.OriginalURL);
        }
        public override string ElementName { get { return "Agent"; } }

        public override string ElementNamePlural { get { return "Agents"; } }
        public override Version Version { get { return new Version(1, 0, 0); } }
        public override bool ValidateTargetPageInputQuery(string input)
        {
            return true;
        }
        public override IEnumerable<FilterComponenetDescription> FiltersDescription
        {
            get
            {
                return new FilterComponenetDescription[]
                {
                    new FilterComponenetDescription()
                    {
                         Header="Nationality",
                         Type= FilterComponenetType.GroupFilter,
                         PropertyName=nameof(Model.Agent.Nationality)
                    },
                    new FilterComponenetDescription()
                    {
                         Header="Trusted",
                         Type= FilterComponenetType.GroupFilter,
                         PropertyName=nameof(Model.Agent.isTrusted)
                    },
                    new FilterComponenetDescription()
                    {
                         Header="Total Properties",
                         Type= FilterComponenetType.RangeFilter,
                         PropertyName=nameof(Model.Agent.TotalProperties),

                    },
                    new FilterComponenetDescription()
                    {
                         Header="Response time",
                         Type= FilterComponenetType.RangeFilter,
                         PropertyName=nameof(Model.Agent.WhatsappResponseTime)
                    },
                    new FilterComponenetDescription()
                    {
                         Header="Experience",
                         Type= FilterComponenetType.RangeFilter,
                         PropertyName=nameof(Model.Agent.YearsOfExperience)
                    }
                };
            }
        }

        public override ScrapingTaskBase GetTask(string targetPage)
        {
            return new PFScrapingTask(targetPage);
        }
    }
}
