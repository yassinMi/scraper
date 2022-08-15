using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;

namespace DNTestPlugin
{
    public class NPMScraper : Plugin
    {
        
        
        public override Type ElementModelType
        {
            get
            {
                return typeof(Topic);
            }
        }

        public override string Name
        {
            get
            {
                return "NPM Docs Scraper";
            }
        }

        public override string TargetHost
        {
            get
            {
                return "npmjs.org";
            }
        }

        public override bool ValidateTargetPageInputQuery(string input)
        {
            return true;
        }

        public override ScrapingTaskBase GetTask(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public override ScrapingTaskBase GetTask(string targetPage)
        {
            return new NpmScrapingTask(targetPage);
        }
    }
}
