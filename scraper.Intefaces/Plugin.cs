using scraper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public abstract class Plugin
    {
        public abstract PluginScrapingTask GetTask(string targetPage);
        public abstract PluginScrapingTask GetTask(TaskInfo taskInfo);
        public abstract string Name { get; }
        public virtual Version Version { get; } = new Version(0,0);
        public virtual string ElementName { get; } = "Element";
        public virtual string ElementNamePlural { get; } = "Elements";
        private ElementDescription _ElementDescription;
        public virtual ElementDescription ElementDescription {
            get {
                if (_ElementDescription == null)
                {
                   
                    _ElementDescription = new ElementDescription()
                    {
                        Fields = ElementModelType.GetProperties().Select(p => new Field()
                        {
                            Name = p.Name,
                            NativeType = p.PropertyType,
                            UIName = CoreUtils.CamelCaseToUIText(p.Name)

                        }),
                        ID = ElementName,
                        Name = ElementName
                    };

                }
                return _ElementDescription;
            }
            set
            {
                _ElementDescription = value;
            }
        }
        public abstract Type ElementModelType { get; }
        public virtual PluginUsageInfo UsageInfo { get; } = null;
        /// <summary>
        /// example:  
        /// if none null it will be part of the default targetPage input Validation function
        /// </summary>
        public abstract string TargetHost { get; }

        public virtual bool ValidateTargetPageInputQuery(string input)
        {
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri) == false)
            {
                return false;
            }
            string website = uri.Host.ToLower();
            if ((TargetHost!=null)&& !((website == TargetHost))) return false;
            return true;
        }
    }

   
    public class ElementDescription 
    {
        public IEnumerable<Field> Fields { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// two plugins with the same Element ID can co exist on the same workspace 
        /// the output data from the different plugis would be consistent and can be then loaded in the viewer without problems
        /// a good ID 
        /// </summary>
        public string ID { get; set; }
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
    public class Field 
    {
        /// <summary>
        /// this must match the Model (type) property name as it's used in the DataGrid binding path
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// currently not used
        /// </summary>
        public Type NativeType { get; set; }
        /// <summary>
        /// will be used in maping properties to the right eement card positions 
        /// </summary>
        public FieldRole Role { get; set; }
        /// <summary>
        /// if not null this will override the tooltip text which is the UIName b default
        /// </summary>
        public string UserDescription { get; set; }
        /// <summary>
        /// not used yet
        /// future usage example: required fields having a null-like value will cause the row to be droped in cleaning processes
        /// </summary>
        public bool IsRequired { get; set; }
        private string _UIName ;
        /// <summary>
        /// visible in the columnt header and other UI places, not setting this propety results in it being automaticall generated from the Name  
        /// </summary>
        public string UIName {
            get
            {
                if (_UIName == null)
                {
                    _UIName = CoreUtils.CamelCaseToUIText(Name);
                }
                return _UIName;
            }
            set
            {
                _UIName = value;
            }
        }
        /// <summary>
        /// the starting column width, (recommended width about 70), the user can always resize columns
        /// </summary>
        public int UIHeaderWidth { get; set; } = 70;
    }

        public abstract class PluginScrapingTask
    {
        public event EventHandler<DownloadingProg> Progress;
        public event EventHandler<string> TaskDetailChanged; //stdout prints something like T: downloading image.jpg
        public event EventHandler<string> Error;  //stderr prints something
        public event EventHandler<string> Resolved;  //fired after obtaining the page html, indicating that the Title is resolved and the page is valid for this scraper
        public event EventHandler<string> PageDone;  //fired when the current page changes, the string provides the page progress like: [4/51]
        public event EventHandler<ScrapTaskStage> StageChanged; //stdout prints something like P: [4/35]
        
        
        protected void OnProgress(DownloadingProg e)
        {
            Progress?.Invoke(this, e);
        }

        protected void OnTaskDetailChanged(string e)
        {
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
            PageDone?.Invoke(this, e);
        }

        protected void OnStageChanged(ScrapTaskStage e)
        {
            StageChanged?.Invoke(this, e);
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
        public DownloadingProg DownloadingProgress { get; set; }
        public TaskStatsInfo TaskStatsInfo { get; set; } = new TaskStatsInfo();
        public ScrapTaskStage Stage { get; set; }
        /// <summary>
        /// pause the task
        /// only supported or downloading data stage
        /// </summary>
        public abstract void Pause();
    }
}
