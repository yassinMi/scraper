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

       

       

        public override PluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            return new FakePluginExternalScrapingTask();
        }

        public override PluginScrapingTask GetTask(string targetPage)
        {
            return new FakePluginExternalScrapingTask() { TargetPage = targetPage };
        }
    }

    public class FakePluginExternalScrapingTask : PluginScrapingTask
    {



      
        

        public override void Pause()
        {

        }
        public override async Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            this.OnStageChanged(Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            this.OnStageChanged(Stage);
        }

        public override async Task RunScraper(CancellationToken ct)
        {
            await Task.Delay(30);

            ResolvedTitle = $"External DLL Test ({this.TargetPage.Substring(0,8)})";
            OnResolved( ResolvedTitle);
            Stage = ScrapTaskStage.DownloadingData;
            this.OnStageChanged( this.Stage);
            for (int i = 0; i < 564; i++)
            {
                await Task.Delay(30);
                OnProgress( new DownloadingProg() { Total = 564, Current = i });
                OnTaskDetailChanged( $"fake download: file{i + 1}.zip");
            }

        }
    }
}
