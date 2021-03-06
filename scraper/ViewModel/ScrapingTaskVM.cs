using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using scraper.Core;
using System.Windows.Input;

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
            };
            m.OnTaskDetail += (s, td) => {
                notif(nameof(CurrentScrapTaskStage));
                this.CurrentTaskDetail = td;
            };
            m.OnError += (s, err) => {
                this.CurrentTaskDetail = err;
            };
            m.OnStageChanged += (s, newStage) => { notif(nameof(CurrentScrapTaskStage)); };
           
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
           
            get { return string.Join(" · "  ,new string[] { "321 Elements", "733 Objects" , "7.8 MB" } ); }
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

        private void hndlPauseCommand()
        {
            Model.Pause();
        }

        private bool canExecutePauseCommand()
        {
            return Model.Stage == ScrapTaskStage.DownloadingData;
        }
    }
}
