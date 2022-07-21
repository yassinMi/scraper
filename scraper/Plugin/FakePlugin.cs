using scraper.Model;
using scraper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Plugin
{
    public class FakePlugin : IPlugin
    {
        public IElementDescription ElementDescription
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ElementName
        {
            get
            {
                return "Fake product";
            }
        }

        public string ElementNamePlural
        {
            get
            {
                return "Fake Products";
            }
        }

        public string Name
        {
            get
            {
                return "FakePlugin";
            }
        }

        public PluginUsageInfo UsageInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        public IPluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public IPluginScrapingTask GetTask(string targetPage)
        {
            return new FakePluginScrapingTask() { TargetPage = targetPage };
        }
    }
    public class FakePluginScrapingTask : IPluginScrapingTask
    {
        /// <summary>
        /// only to be instantiated through the IPlugin instance (using IPlugin.getScrapingTask)
        /// </summary>
        public FakePluginScrapingTask()
        {

        }

        public DownloadingProg DownloadingProgress { get; set; }


        public string ResolvedTitle { get; set; } = null;

        public ScrapTaskStage Stage { get; set; }

        public string TargetPage { get; set; } = null;

        public event EventHandler<string> OnError;
        public event EventHandler<string> OnPage;
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnResolved;
        public event EventHandler<ScrapTaskStage> OnStageChanged;
        public event EventHandler<string> OnTaskDetail;

        public void Pause()
        {
            throw new NotImplementedException();
        }

        async public Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            this.OnStageChanged?.Invoke(this, this.Stage);

        }

        async public Task RunScraper()
        {
           

            await Task.Delay(30);
            
            ResolvedTitle = "Fake Books Page (1/3)";
            OnResolved?.Invoke(this, ResolvedTitle);
            Stage = ScrapTaskStage.DownloadingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(30);
                OnProgress?.Invoke(this, new DownloadingProg() { Total = 50, Current = i });
                OnTaskDetail?.Invoke(this, $"fake download: file{i + 1}.zip");
            }

           

        }
    }
}
