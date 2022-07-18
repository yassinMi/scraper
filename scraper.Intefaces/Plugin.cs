using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
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
        string ElementName { get; }
        string ElementNamePlural { get; }
        IElementDescription ElementDescription { get; }
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
        ScrapTaskStage Stage { get; set; }
        /// <summary>
        /// pause the task
        /// only supported or downloading data stage
        /// </summary>
        void Pause();
    }
}
