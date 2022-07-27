using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;
using System.Threading;

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
                    new Field() {Name= "fakeName",  NativeType= typeof(string), IsRequired=true },
                    new Field() {Name= "fakePrice", NativeType= typeof(double), IsRequired=true },
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

       

       

        public override IPluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            return new FakePluginExternalScrapingTask();
        }

        public override IPluginScrapingTask GetTask(string targetPage)
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

        public async Task RunScraper(CancellationToken ct)
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
