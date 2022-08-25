using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace scraper.Core
{
    /// <summary>
    /// the json format used in the process of saving/retrieving tasks in the worksapace/.scraper/tasks folder as json files
    /// the feature is not fully implemented yet
    /// </summary>
    public class TaskInfo
    {
        public ScrapTaskStage Stage;
        public string OriginalURL;
        public string Title;
        public string OutputCSVFilename;
        public string TargetPageSavedHtmlFilename;

        public string[] DownloadedIDS;
        public DownloadingProg DownloadingProgress;
        public bool IsCompleted;
    }

    /// <summary>
    /// represents a scraping task, wraping the main plugin functionality: downloading, parsing and saving results and objects.
    /// </summary>
    public abstract class ScrapingTaskBase
    {
        public event EventHandler<DownloadingProg> Progress;
        public event EventHandler<string> TaskDetailChanged; //stdout prints something like T: downloading image.jpg
        public event EventHandler<string> Error;  //stderr prints something
        public event EventHandler<string> Resolved;  //fired after obtaining the page html, indicating that the Title is resolved and the page is valid for this scraper
        public event EventHandler<string> PageDone;  //fired when the current page changes, the string provides the page progress like: [4/51]
        public event EventHandler<ScrapTaskStage> StageChanged; //stdout prints something like P: [4/35]
        public event EventHandler<int> BrowserWindowsCountChanged; //
        public event EventHandler<bool> IsStopRequestedValueChanged; //
        public event EventHandler<bool> IsStopEnabledValuehanged; //



        protected void OnProgress(DownloadingProg e)
        {
            Progress?.Invoke(this, e);
        }

        protected void OnTaskDetailChanged(string e)
        {
            TaskDetail = e;
            TaskDetailChanged?.Invoke(this, e);
        }

        protected void OnError(string e)
        {
            Error?.Invoke(this, e);
        }

        protected void OnResolved(string e)
        {
            Resolved?.Invoke(this, e);
        }

        protected void OnPageStarted(string e)
        {
            Page = e;
            PageDone?.Invoke(this, e);
        }

        protected void OnStageChanged(ScrapTaskStage e)
        {
            Stage = e;
            StageChanged?.Invoke(this, e);
        }

        protected void OnBrowserWindowsCountChanged(int cc)
        {
            BrowserWindowsCount = cc;
            BrowserWindowsCountChanged?.Invoke(this, cc);
        }

        protected void OnIsStopRequestedChanged(bool val)
        {
            IsStopRequested = val;
            IsStopRequestedValueChanged?.Invoke(this, IsStopRequested);
        }
        protected void OnIsStopEnabledd(bool val)
        {
            IsStopeEnabled = val;
            IsStopEnabledValuehanged?.Invoke(this, IsStopRequested);
        }
        /// <summary>
        /// downloads the raw objects containing the required flieds, saves them under workspace/raw or workspace/{ElementName}-raw
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        public abstract Task RunScraper(CancellationToken ct);
        /// <summary>
        /// not used yet (not called from the scraper project), RunScraper must do all the task stages
        /// </summary>
        /// <returns></returns>
        public abstract Task RunConverter();
        /// <summary>
        /// the Title of the target page as in specification this is resolved before starting download
        /// </summary>
        public string ResolvedTitle { get; set; }
        public string TargetPage { get; set; }
        /// <summary>
        /// consumers can specify output file (full path) , otherwise it's automatically determined from the page title or else (plugin implementation)
        /// </summary>
        public string DesiredOutputFile { get; set; }
        /// <summary>
        /// files to whch the csv results are written
        /// </summary>
        public string ActualOutputFile { get; protected set; }
        public DownloadingProg DownloadingProgress { get; set; }
        public TaskStatsInfo TaskStatsInfo { get; set; } = new TaskStatsInfo();
        public ScrapTaskStage Stage { get; set; }
        /// <summary>
        /// used for all stages, to indicate the current process, e,g in setup stage it can be "starting chrome driver"
        /// </summary>
        public string TaskDetail { get; set; }
        
        public int BrowserWindowsCount { get; set; }
        /// <summary>
        /// used for feedback to disabe Stop button as the stoping process is carried out
        /// </summary>
        public bool IsStopRequested { get; internal set; }
        /// <summary>
        /// similar to IsStopRequested but causes the button to be hidden
        /// </summary>
        public bool IsStopeEnabled { get; internal set; }
        public string Page { get; internal set; }
        /// <summary>
        /// pause the task
        /// only supported or downloading data stage
        /// </summary>
        public abstract void Pause();
    }

}
