using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using scraper.Core;
using System.Windows.Input;
using scraper.Services;
using System.Threading;

namespace scraper.ViewModel
{
    public class ScrapingTaskVM : BaseViewModel
    {
        public ScrapingTaskVM(ScrapingTaskBase m)
        {
            Model = m;
            DownloadProgress = m.DownloadingProgress;
            m.Resolved += (s, t) =>
            {
                Title = t;
            };
            
            notif(nameof(CurrentScrapTaskStage));
            m.Progress += (s, p) => {
                this.DownloadProgress = p;
                notif(nameof(StatsInfo));
                notif(nameof(Info));
            };
            m.TaskDetailChanged += (s, td) => {
                notif(nameof(CurrentScrapTaskStage));
                this.CurrentTaskDetail = td;
            };
            m.Error += (s, err) => {
                this.CurrentTaskDetail = err;
                this.FailingReason = err;
            };
            m.StageChanged += (s, newStage) => { notif(nameof(CurrentScrapTaskStage)); };
            m.PageDone+=(s,p)=> {
                CurrentPage = p;
                notif(nameof(CurrentPage)); };
            m.BrowserWindowsCountChanged += (s, cc) =>
            {
                this.BrowserWindowsCount = cc;
            };

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

        private ScrapingTaskBase _Model;

        public ScrapingTaskBase Model
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


        private int _BrowserWindowsCount;
        /// <summary>
        /// number of open browser windows(tabs) 
        /// </summary>
        public int BrowserWindowsCount
        {
            set { _BrowserWindowsCount = value; notif(nameof(BrowserWindowsCount)); }
            get { return _BrowserWindowsCount; }
        }



        public ICommand PauseCommand { get { return new Mi.Common.MICommand(this.hndlPauseCommand, this.canExecutePauseCommand); } }

        public string CurrentPage { get; set; }

        private void hndlPauseCommand()
        {
            currentCTS?.Cancel();
        }

        private bool canExecutePauseCommand()
        {
            return ((Model?.Stage) != null) && ( Model.Stage ==ScrapTaskStage.DownloadingData);
        }
        public CancellationTokenSource currentCTS { get; set; }

        public ICommand RetryCommand { get { return new Mi.Common.MICommand(hndlRetryCommand); } }

        private  void hndlRetryCommand()
        {
            currentCTS = new CancellationTokenSource();
            var t = new Task(async () =>
            {
                await Model.RunScraper(currentCTS.Token);
            }
            );
            t.Start();
        }
    }
}
