﻿using scraper.Core;
using scraper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Workspace
{
    public class CSVResource
    {
        public Uri Path { get; set; }
        public int Rows { get; set; }
        public bool isChecked { get; set; } = false;
        public bool isRemoved { get; set; }
        public bool isbadFormat { get; set; }
        public void Check()
        {
            isChecked = true;
            if (!File.Exists(Path.OriginalString)) { isRemoved = true; return; }
            isRemoved = false;
            int total_rows = 0;
            int valid_rows = 0;
            isbadFormat = ! CSVUtils.checkCSV(Path.OriginalString, out total_rows, out valid_rows);
            Debug.WriteLine("cheking returned: cont " + total_rows);
                   Debug.WriteLine("cheking returned: valid " + valid_rows);


            Rows = valid_rows;
        }
    }
    /// <summary>
    /// for customizing the folders structure, except anything under .scraper/ is hard coded and not customizable.
    /// </summary>
    public class WorkspaceOptions
    {
        private  WorkspaceOptions()
        {
            TargetPagesRelativeLocation = "target-pages";
            ElementsHTMLRelativeLocation = @"companies-raw\html";
            ElementsImagesRelativeLocation = @"companies-raw\img";
            CSVOutputRelativeLocation = @"csv";
        }
        public string TargetPagesRelativeLocation { get; }
        public string ElementsHTMLRelativeLocation { get; }
        public string ElementsImagesRelativeLocation { get; }

        public static WorkspaceOptions Default = new WorkspaceOptions() ;
        public   string CSVOutputRelativeLocation { get; }
    }


    /// <summary>
    /// also the app root data source
    /// </summary>
    public sealed class Workspace
    {
        private static Workspace _current = null;    
        public static Workspace Current { get {
                if (_current == null)
                {
                    throw new Exception("auto instantiating the current workspace is no longer supported");
                }
                return _current;
            }
        }

        public WorkspaceOptions Options { get; set; } = WorkspaceOptions.Default;
        public  string Directory { get; set; }
        public  List<CSVResource> CSVResources { get; set; }
        /// <summary>
        /// the main plugin instance, used t carry out most of business operations. loaded only once at startup
        /// </summary>
        public IPlugin Plugin { get; set; }
        /// <summary>
        /// to be called only once, internaly startup
        /// 
        /// taking a workspacePath argument is for unit test purposes
        /// if workspacePath is ommited it isobtained from the current path in config
        /// </summary>
        public static Workspace GetWorkspace(string workspacePath )
        {
            if (workspacePath == null) throw new Exception("workspacePath cannot be null"); // workspacePath = ConfigService.Instance.WorkspaceDirectory;
            //creating the sub directories
            Workspace res = new Workspace() { Directory = workspacePath};
            res.CSVResources = new List<CSVResource>();
            if (!System.IO.Directory.Exists(res.Directory))
            {
                throw new Exception("workspace directory doesn't exist");
            }
            SetUpWorkspaceFolders(res);
            var all_file_in_csv = System.IO.Directory.GetFiles(res.CSVOutputFolder);
            foreach (var item in all_file_in_csv)
            {
                if (Path.GetExtension(item).ToLower().Replace(".", "") == "csv")
                {
                    var o = new CSVResource() { Path = new Uri(item) };
                    o.Check();
                    res.CSVResources.Add(o);
                }
            }
            return res;
        }
        /// <summary>
        /// currently this only usfull in testing
        /// calling this assigns the Current property a value with the workspace path specified, 
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <returns></returns>
        public static void MakeCurrent(string workspacePath )
        {
            Debug.WriteLine("updating Workspace.current: " + workspacePath);
            _current = GetWorkspace(workspacePath);
        }

        private static void SetUpWorkspaceFolders(Workspace workspace)
        {
            System.IO.Directory.CreateDirectory(Path.Combine(workspace.Directory, workspace.CSVOutputFolder));

            System.IO.Directory.CreateDirectory(Path.Combine(workspace.Directory, @".scraper\tasks"));
            System.IO.Directory.CreateDirectory(Path.Combine(workspace.Directory, workspace.ElementsImagesFolder));
            System.IO.Directory.CreateDirectory(Path.Combine(workspace.Directory, workspace.HtmlObjectsFolder));
            System.IO.Directory.CreateDirectory(Path.Combine(workspace.Directory, workspace.TPFolder));

        }
        /// <summary>
        /// alter the Current object so that csv list is up to date
        /// </summary>
        public void refresh()
        {
            
            CSVResources = new List<CSVResource>();
            
            if (!System.IO.Directory.Exists(Directory))
            {
                throw new Exception("Workspace folder may have been removed since workspace creation time.");
            }
            SetUpWorkspaceFolders(this);
            var all_file_in_csv = System.IO.Directory.GetFiles(System.IO.Path.Combine(CSVOutputFolder));
            foreach (var item in all_file_in_csv)
            {
                if (Path.GetExtension(item).ToLower().Replace(".", "") == "csv")
                {
                    var o = new CSVResource() { Path = new Uri(item) };
                    o.Check();
                    CSVResources.Add(o);
                }
            }
        }

        public string GetTasksDirectory()
        {
            return (System.IO.Path.Combine(Directory, ".scraper/tasks"));

        }

        public IEnumerable<IPluginScrapingTask> GetScrapingTasksFromFiles()
        {
            Console.WriteLine("mlkml");
            string[] all_file_in_csv;
            try
            {
                all_file_in_csv = System.IO.Directory.GetFiles(GetTasksDirectory());
            }
            catch (DirectoryNotFoundException )
            {
                System.Diagnostics.Debug.WriteLine("not found dir: " + GetTasksDirectory());
                yield break;
            }
            
            foreach (var item in all_file_in_csv)
            {
                if (Path.GetExtension(item).ToLower().Replace(".", "") == "json")
                {
                    Debug.WriteLine("LoadFromFile");
                    //System.Text.Json k;
                    try
                    {
                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskInfo>(System.IO.File.ReadAllText(item));
                        Debug.WriteLine(obj.DownloadingProgress);
                        //return Reflection ;
                    }
                    catch (Exception)
                    {

                       // return new ScrapingTaskModel() { Title = "failed" };
                    }
                    //yield return m;
                    //todo repair this using nterface
                    yield break;
                }
            }
        }


        /// <summary>
        /// text file under .scraper/ that lists the Names of the plugins used in the workdpace (separated by envrements.newLine)
        /// </summary>
        public string PluginsPtrFilePath { get { return Path.Combine(this.Directory, ".scraper/plugins"); } }
        public string ElementsImagesFolder { get { return Path.Combine(this.Directory, Options.ElementsImagesRelativeLocation); } }

        public string HtmlObjectsFolder { get { return Path.Combine(this.Directory, Options.ElementsHTMLRelativeLocation); } }
        public string CSVOutputFolder { get { return Path.Combine(this.Directory, Options.CSVOutputRelativeLocation);} }
        public string TPFolder { get { return Path.Combine(this.Directory, Options.TargetPagesRelativeLocation); } }
       
        
    }
}