using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Interfaces
{

    public enum ScrapTaskStage { Ready, DownloadingData, Paused, ConvertingData, Success, Failed }


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
    /// supported pattern is: download a page's static elements
    /// </summary>
    public interface IPlugin
    {
        IPluginScrapingTask GetTask(string targetPage);
        IPluginScrapingTask GetTask(TaskInfo taskInfo);
        string Name { get; }
        Version Version { get; }


    }


    public interface IPluginScrapingTask
    {
        event EventHandler<DownloadingProg> OnProgress;
        event EventHandler<string> OnTaskDetail; //stdout prints something like T: downloading image.jpg
        event EventHandler<string> OnError;  //stderr prints something
        event EventHandler<string> OnResolved;  //fired after obtaining the page html, indicating that the Title is resolved and the page is valid for this scraper
        event EventHandler<ScrapTaskStage> OnStageChanged; //stdout prints something like P: [4/35]
        /// <summary>
        /// downloads the raw objects containing the required flieds, saves them under workspace/raw or workspace/{ElementName}-raw
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        Task RunScraper();
        Task RunConverter();
        /// <summary>
        /// the Title of the target page as in specificationn this is resolved before starting download
        /// </summary>
        string ResolvedTitle { get; set; }
        string TargetPage { get; set; }
        DownloadingProg DownloadingProgress { get; set; }
        ScrapTaskStage Stage { get; set; }
    }
}
