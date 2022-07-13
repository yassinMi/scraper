using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using scraper.Model.Common;

namespace scraper.ViewModel
{
    public class ScrapingTaskVM : BaseViewModel
    {
        public ScrapingTaskVM(ScrapingTaskModel m)
        {
            Model = m;
            DownloadProgress = m.DownloadingProgress;
            Title = m.Title;
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
            m.OnStageChange += (s, st) =>
            {
                notif(nameof(CurrentScrapTaskStage));
            };
        }
        public ScrapingTaskVM()
        {
            Title = "Design Time";
            Model = new ScrapingTaskModel() { Stage = ScrapTaskStage.Success };
            DownloadProgress = new DownloadingProg() { Total = 45, Current = 30 };
            notif(nameof(CurrentScrapTaskStage));

        }


        private string _Title;
        public string Title
        {
            set { _Title = value; notif(nameof(Title)); }
            get { return _Title; }
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

        private ScrapingTaskModel _Model;

        public ScrapingTaskModel Model
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


    }
}
