using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;

namespace FakePluginExternal
{
    public class FakePluginExternal : IPlugin
    {
        public IElementDescription ElementDescription
        {
            get
            {
                return new ElementDescription()
                {
                    Name = "Fake Product",
                    Fields = new Field[]
                {
                    new Field() {Name= "Price", NativeType= typeof(double), IsRequired=true },
                    new Field() {Name= "Price", NativeType= typeof(double), IsRequired=true },
                }.Cast<IField>(),
                    ID = "Fake Product 1"
                };
            }
        }

        public string ElementName
        {
            get
            {
                return "Fake Product";
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
                return "FakePluginExternal";
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
                return new Version();
            }
        }

        public IPluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            return new FakePluginExternalScrapingTask();
        }

        public IPluginScrapingTask GetTask(string targetPage)
        {
            return new FakePluginExternalScrapingTask() { TargetPage = targetPage };
        }
    }

    public class FakePluginExternalScrapingTask : IPluginScrapingTask
    {
        public DownloadingProg DownloadingProgress { get; set; }
        public string ResolvedTitle { get; set; }

        public ScrapTaskStage Stage { get; set; }

        public string TargetPage { get; set; }

        public TaskStatsInfo TaskStatsInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<string> OnError;
        public event EventHandler<string> OnPage;
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnResolved;
        public event EventHandler<ScrapTaskStage> OnStageChanged;
        public event EventHandler<string> OnTaskDetail;

        public void Pause()
        {

        }
        public async Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            this.OnStageChanged?.Invoke(this, this.Stage);
        }

        public async Task RunScraper()
        {
            await Task.Delay(30);

            ResolvedTitle = $"External DLL Test ({this.TargetPage.Substring(0,8)})";
            OnResolved?.Invoke(this, ResolvedTitle);
            Stage = ScrapTaskStage.DownloadingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            for (int i = 0; i < 564; i++)
            {
                await Task.Delay(30);
                OnProgress?.Invoke(this, new DownloadingProg() { Total = 564, Current = i });
                OnTaskDetail?.Invoke(this, $"fake download: file{i + 1}.zip");
            }

        }
    }
}
