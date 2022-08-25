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
        public override Version Version { get { return new Version(1, 0, 1); } }
        public override bool ValidateTargetPageInputQuery(string input)
        {
            //https://www.propertyfinder.ae/en/find-agent/search
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri) == false)
            {
                return false;
            }
            string website = uri.Host.ToLower();
            if (!((website == TargetHost))) return false;
            if (!input.Contains("/en/find-agent/search")) return false;
            return true;
        }
        public override PluginUsageInfo UsageInfo
        {
            get
            {
                return new PluginUsageInfo()
                {
                    UsageInfoViewHeader = "Supported URL's:",
                    UseCases = new TargetPageUrlUseCaseHelp[]
                      {
                          new TargetPageUrlUseCaseHelp()
                          {
                               Description="Agents Search Results",
                               ExampleUrls = new string[]
                               {
                                   "https://www.propertyfinder.ae/en/find-agent/search",
                                   "https://www.propertyfinder.ae/en/find-agent/search?page=340",
                                   "https://www.propertyfinder.ae/en/find-agent/search?location_id=3&order_by=-trusted_score&text=Ras%20Al%20Khaimah",
                               }
                          }
                      }
                };
            }
        }
        public override IEnumerable<FilterComponenetDescription> FiltersDescription
        {
            get
            {
                return new FilterComponenetDescription[]
                {
                    
                    
                    new FilterComponenetDescription()
                    {
                         Header="Total Properties",
                         Type= FilterComponenetType.RangeFilter,
                         PropertyName=nameof(Model.Agent.TotalProperties),
                         MinMaxValidator= (inp)=> {
                             int inp_;
                             if(!int.TryParse(inp,out inp_)) return new Tuple<bool, string>(false,"not a number!");
                             return new Tuple<bool, string>(true, null);
                         }, 
                         IsInRange=(min,max,obj)=>
                         {
                             int min_,max_;
                             if(!int.TryParse(min,out min_)) return true;
                             if(!int.TryParse(max,out max_)) return true;
                             if ((obj as Agent)==null) return true;
                             int v;
                             if(!int.TryParse((obj as Agent).TotalProperties,out v)) return false;
                             return (v<=max_)
                             &&(v>=min_);
                         }
                    },
                   
                    new FilterComponenetDescription()
                    {
                         Header="Experience",
                         Type= FilterComponenetType.RangeFilter,
                         PropertyName=nameof(Model.Agent.YearsOfExperience),
                         MinMaxValidator= (inp)=> {
                             int inp_;
                             if(!int.TryParse(inp,out inp_)) return new Tuple<bool, string>(false,"not a number!");
                             return new Tuple<bool, string>(true, null);
                         },
                         IsInRange=(min,max,obj)=>
                         {
                             int min_,max_;
                             if(!int.TryParse(min,out min_)) return true;
                             if(!int.TryParse(max,out max_)) return true;
                             if ((obj as Agent)==null) return true;
                             int v;
                             if(!int.TryParse((obj as Agent).YearsOfExperience,out v)) return false;
                             return (v<=max_)
                             &&(v>=min_);
                         }
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
