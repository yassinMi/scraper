using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using scraper.Core;
using System.Windows.Input;
using scraper.Services;

namespace scraper.ViewModel
{
    public class ScrapingTaskVM : BaseViewModel
    {
        public ScrapingTaskVM(IPluginScrapingTask m)
        {
            Model = m;
            DownloadProgress = m.DownloadingProgress;
            m.OnResolved += (s, t) =>
            {
                Title = t;
            };
            
            notif(nameof(CurrentScrapTaskStage));
            m.OnProgress += (s, p) => {
                this.DownloadProgress = p;
                notif(nameof(StatsInfo));
                notif(nameof(Info));
            };
            m.OnTaskDetail += (s, td) => {
                notif(nameof(CurrentScrapTaskStage));
                this.CurrentTaskDetail = td;
            };
            m.OnError += (s, err) => {
                this.CurrentTaskDetail = err;
                this.FailingReason = err;
            };
            m.OnStageChanged += (s, newStage) => { notif(nameof(CurrentScrapTaskStage)); };
            m.OnPage+=(s,p)=> {
                CurrentPage = p;
                notif(nameof(CurrentPage)); };

        }
        public ScrapingTaskVM()
        {
            
            

        }


        private string _Title;
        public string Title
        {
            set { _Title = value; notif(nameof(Title)); }
            get { return _Title; }
        }

        private string _Info;
        public string Info
        {
           
            get { return string.Join(" · "  ,new string[] { $"{StatsInfo.Elements} Elements", $"{StatsInfo.Objects} Objects" , Utils.BytesToString(StatsInfo.TotalSize)} ); }
        }


        public TaskStatsInfo StatsInfo
        {
            
            get { return Model.TaskStatsInfo; }
        }



        private string _FailingReason;
        public string FailingReason
        {
            set { _FailingReason = value; notif(nameof(FailingReason)); }
            get { return _FailingReason; }
        }


        private string _CurrentTaskDetail;
        public string CurrentTaskDetail
        {
            set { _CurrentTaskDetail = value; notif(nameof(CurrentTaskDetail)); }
            get { return _CurrentTaskDetail; }
        }



        private DownloadingProg _DownloadProgress;

        public DownloadingProg DownloadProgress
        {
            set { _DownloadProgress = value; notif(nameof(DownloadProgress)); }
            get { return _DownloadProgress; }
        }

        private IPluginScrapingTask _Model;

        public IPluginScrapingTask Model
        {
            set { _Model = value; notif(nameof(Model)); }
            get { return _Model; }
        }

        public ScrapTaskStage CurrentScrapTaskStage
        {
            get
            {
                return Model.Stage;
            }
        }

        public ICommand PauseCommand { get { return new Mi.Common.MICommand(this.hndlPauseCommand, this.canExecutePauseCommand); } }

        public string CurrentPage { get; set; }

        private void hndlPauseCommand()
        {
            Model.Pause();
        }

        private bool canExecutePauseCommand()
        {
            return Model.Stage == ScrapTaskStage.DownloadingData;
        }

        public ICommand RetryCommand { get { return new Mi.Common.MICommand(hndlRetryCommand); } }

        private  void hndlRetryCommand()
        {

            var t = new Task(async () =>
            {
                await Model.RunScraper();
            }
            );
            t.Start();
        }
    }
}
