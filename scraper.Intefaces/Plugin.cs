using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{

    public enum ScrapTaskStage { Ready, DownloadingData, Paused, ConvertingData, Success, Failed }


    public class TargetPageUrlUseCaseHelp
    {
        public IEnumerable<string> ExampleUrls { get; set; }
        public string Description { get; set; }
    }
    public class PluginUsageInfo
    {
        public PluginUsageInfo()
        {
            UsageInfoViewHeader = "Supported URL's:";
            UseCases = new TargetPageUrlUseCaseHelp[] {
                    new TargetPageUrlUseCaseHelp() {Description="You can scrap category pages", ExampleUrls= new string[] { "https://www.businesslist.ph/cat/manila/3", "https://www.businesslist.ph/cat/manila/2" } },
                    new TargetPageUrlUseCaseHelp() {Description="You can scrap locations pages",

            ExampleUrls = new string[] {
                     "https://www.businesslist.ph/location/manila/3",
                        "https://www.businesslist.ph/location/manila/2" }},
                };
        }
        public string UsageInfoViewHeader { get; set; }
        public IEnumerable<TargetPageUrlUseCaseHelp> UseCases { get; set; }
    }

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
        string ElementName { get; }
        string ElementNamePlural { get; }
        IElementDescription ElementDescription { get; }
        PluginUsageInfo UsageInfo { get; }
    }

    public interface IElementDescription
    {
        IEnumerable<IField> Fields { get; }
        string Name { get; }
        /// <summary>
        /// two plugins with the same Element ID can co exist on the same workspace 
        /// the output data from the different plugis would be consistent and can be then loaded in the viewer without problems
        /// a good ID 
        /// </summary>
        string ID { get; }


    }
    public class ElementDescription : IElementDescription
    {
        public IEnumerable<IField> Fields { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
    }

    public interface IField
    {
        string Name { get; }
        /// <summary>
        /// for the user information (used as tooltips)
        /// </summary>
        string UserDescription { get; }
        Type NativeType { get; }
        FieldRole Role { get; }
        /// <summary>
        /// useage example: required fields having a null-like value will cause the row to be droped in cleaning processes
        /// </summary>
        bool IsRequired { get; }
    }

    public class TaskStatsInfo
    {
        /// <summary>
        /// number of objects written locally since the start of the task
        /// </summary>
        public int Objects { get; set; }
        /// <summary>
        /// all treated ellements since the start of the task
        /// </summary>
        public int Elements { get; set; }
        /// <summary>
        /// approxmiated total size of the local saved objects + target pages files (for user information)
        /// </summary>
        public int TotalSize { get; set; }

        public DateTime StartTime { get; set; }

        public void Reset()
        {
            StartTime = DateTime.Now;
            Objects = 0; Elements = 0; TotalSize = 0;

        }

        public void incObject(int newObjects, int newBytes = 0)
        {
            Objects += newObjects;
            TotalSize += newBytes;
        }
        public void incSize(int newBytes)
        {
            TotalSize += newBytes;
        }
        public void incElem(int number)
        {
            Elements += number;
        }
    }
    public struct Field : IField
    {
        public string Name { get; set; }
        
        public Type NativeType { get; set; }

        public FieldRole Role { get; set; }

        public string UserDescription { get; set; }
        public bool IsRequired { get; set; } 
    }

    public interface IPluginScrapingTask
    {
        event EventHandler<DownloadingProg> OnProgress;
        event EventHandler<string> OnTaskDetail; //stdout prints something like T: downloading image.jpg
        event EventHandler<string> OnError;  //stderr prints something
        event EventHandler<string> OnResolved;  //fired after obtaining the page html, indicating that the Title is resolved and the page is valid for this scraper
        event EventHandler<string> OnPage;  //fired when the current page changes, the string provides the page progress like: [4/51]

        event EventHandler<ScrapTaskStage> OnStageChanged; //stdout prints something like P: [4/35]
        /// <summary>
        /// downloads the raw objects containing the required flieds, saves them under workspace/raw or workspace/{ElementName}-raw
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        Task RunScraper();
        Task RunConverter();
        /// <summary>
        /// the Title of the target page as in specification this is resolved before starting download
        /// </summary>
        string ResolvedTitle { get; set; }
        string TargetPage { get; set; }
        DownloadingProg DownloadingProgress { get; set; }
        TaskStatsInfo TaskStatsInfo { get; set; }
        ScrapTaskStage Stage { get; set; }
        /// <summary>
        /// pause the task
        /// only supported or downloading data stage
        /// </summary>
        void Pause();
    }
}
