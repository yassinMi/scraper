using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;
using System.Threading;
using FakePluginExternal.Model;
using System.Text.RegularExpressions;
using scraper.Core.Utils;
using System.Collections;

namespace FakePluginExternal
{
    public class FakePluginExternal : Plugin
    {
        public override ElementDescription ElementDescription
        {
            get
            {
                return new ElementDescription()
                {
                    Name = "Fake Product",
                    Fields = new Field[]
                {
                    new Field() {
                        Name = nameof(FakePlugnExternalElementModel.name),
                        NativeType = typeof(string), IsRequired=true
                    },
                    new Field() {
                        Name = nameof(FakePlugnExternalElementModel.price),
                        NativeType = typeof(double), IsRequired=true
                    },
                    new Field() {
                        Name = nameof(FakePlugnExternalElementModel.category),
                        NativeType = typeof(double), IsRequired=true
                    },
                },
                    ID = "Fake Product 1"
                };
            }
        }

        public override Type ElementModelType
        {
            get
            {
                return typeof(Model.FakePlugnExternalElementModel);
            }
        }

        public override string ElementName
        {
            get
            {
                return "Fake Product";
            }
        }

        public override string ElementNamePlural
        {
            get
            {
                return "Fake Products";
            }
        }

        public override string Name
        {
            get
            {
                return "FakePluginExternal";
            }
        }

       

       

        public override PluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            return new FakePluginExternalScrapingTask();
        }

        public override PluginScrapingTask GetTask(string targetPage)
        {
            return new FakePluginExternalScrapingTask() { TargetPage = targetPage };
        }
        public override PluginUsageInfo UsageInfo
        {
            get
            {
                return new PluginUsageInfo() {
                     UsageInfoViewHeader="This is a fake plugin",
                     UseCases= new TargetPageUrlUseCaseHelp[]
                     {
                         new TargetPageUrlUseCaseHelp()
                         {
                              Description="it will generate fake task & elements based on the url:",
                              ExampleUrls = new string[]
                              {
                                  "fake.com/?title=All%20Monitors&pages=9&prodectsPerPage=45&delayms=5",
                              }
                         }
                     }
                };
            }
        }

        public override string TargetHost
        {
            get
            {
                return "fake.com";
            }
        }
    }

    public class FakePluginExternalScrapingTask : PluginScrapingTask
    {



      
        

        public override void Pause()
        {

        }
        public override async Task RunConverter()
        {
            
            
        }

        public override async Task RunScraper(CancellationToken ct)
        {
            await Task.Delay(30);
            Uri u = new Uri(TargetPage);
            var coll = CoreUtils.parseQueryString(u.Query);
            int pages = 1;
            string title = "Fake Products Page";
            int delayms = 200;
            int productsPerPage = 100;
            title = coll["title"] ?? title;
            if (coll["pages"] != null) pages = int.Parse(coll["pages"]);
            if (coll["delayms"] != null) delayms = int.Parse(coll["delayms"]);
            if (coll["productsPerPage"] != null) productsPerPage = int.Parse(coll["productsPerPage"]);
            ResolvedTitle = $"{title}";
            OnResolved(ResolvedTitle);
            Stage = ScrapTaskStage.DownloadingData;
            OnStageChanged(Stage);
            foreach (var item in Enumerable.Range(0, pages))
            {
                OnPageStarted($"[page {item+1}/{pages}]");
                for (int i = 0; i < productsPerPage; i++)
                {
                    await Task.Delay(delayms);
                    OnProgress(new DownloadingProg() {  Total = productsPerPage, Current = i });
                    OnTaskDetailChanged($"fake download: file{i + 1}.zip");
                    this.TaskStatsInfo.incObject(2, 454257);
                    TaskStatsInfo.incElem(1);
                }
                
            }
            OnTaskDetailChanged(null);
            Stage = ScrapTaskStage.Success;
            OnStageChanged(Stage);
            
            
            
            

        }
    }
}
