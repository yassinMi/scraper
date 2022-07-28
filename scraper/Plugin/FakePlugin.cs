using scraper.Model;
using scraper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace scraper.Plugin
{
    public class FakePlugin : Core.Plugin
    {
        public override ElementDescription ElementDescription
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type ElementModelType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string ElementName
        {
            get
            {
                return "Fake product";
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
                return "FakePlugin";
            }
        }

        public override PluginUsageInfo UsageInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        public override PluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public override PluginScrapingTask GetTask(string targetPage)
        {
            return new FakePluginScrapingTask() { TargetPage = targetPage };
        }
    }
    public class FakePluginScrapingTask : PluginScrapingTask
    {
        /// <summary>
        /// only to be instantiated through the IPlugin instance (using IPlugin.getScrapingTask)
        /// </summary>
        public FakePluginScrapingTask()
        {

        }       

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        async override public Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            OnStageChanged(Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            OnStageChanged(Stage);

        }

        async override public Task RunScraper(CancellationToken ct)
        {
           

            await Task.Delay(30);
            
            ResolvedTitle = "Fake Books Page (1/3)";
            OnResolved(ResolvedTitle);
            Stage = ScrapTaskStage.DownloadingData;
            OnStageChanged(Stage);
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(30);
                OnProgress(new DownloadingProg() { Total = 50, Current = i });
                OnTaskDetailChanged( $"fake download: file{i + 1}.zip");
            }

           

        }
    }
}
