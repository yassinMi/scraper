using Mi.Common;
using scraper.Core;
using scraper.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
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
            isbadFormat = ! Utils.checkCSV(Path.OriginalString, out total_rows, out valid_rows);
            Debug.WriteLine("cheking returned: cont " + total_rows);
                   Debug.WriteLine("cheking returned: valid " + valid_rows);


            Rows = valid_rows;
        }
    }
    /// <summary>
    /// also the app root data source
    /// </summary>
    public class Workspace
    {
        private static Workspace _current = null;
        
        public static Workspace Current { get {
                if (_current == null)
                {
                    _current = GetWorkspace();
                }
                return _current;
            }
            }
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
        public static Workspace GetWorkspace(string workspacePath = null)
        {
            if (workspacePath == null) workspacePath = ConfigService.Instance.WorkspaceDirectory;
            
            //creating the sub directories
            Workspace res = new Workspace() { Directory = workspacePath};
            res.CSVResources = new List<CSVResource>();
            if (!System.IO.Directory.Exists(res.Directory))
            {
                throw new Exception("workspace directory doesn't exist");
            }
            SetUpWorkspaceFolders(res.Directory);
            var all_file_in_csv = System.IO.Directory.GetFiles(System.IO.Path.Combine(res.Directory, ConfigService.Instance.CSVOutputRelativeLocation));
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
            _current = GetWorkspace(workspacePath);
        }

        private static void SetUpWorkspaceFolders(string workspacePath)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(workspacePath, ConfigService.Instance.CSVOutputRelativeLocation));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(workspacePath, ConfigService.Instance.ProductsImagesRelativeLocation));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(workspacePath, ConfigService.Instance.ProductsHTMLRelativeLocation));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(workspacePath, ConfigService.Instance.TargetPagesRelativeLocation));

        }
        /// <summary>
        /// alter the Current object so that csv list is up to date
        /// </summary>
        internal void refresh()
        {
            
            CSVResources = new List<CSVResource>();
            
            if (!System.IO.Directory.Exists(Directory))
            {

            }
            SetUpWorkspaceFolders(Directory);
            var all_file_in_csv = System.IO.Directory.GetFiles(System.IO.Path.Combine(Directory, ConfigService.Instance.CSVOutputRelativeLocation));
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

        private string GetTasksDirectory()
        {
            return (System.IO.Path.Combine(ConfigService.Instance.WorkspaceDirectory, ".scraper/tasks"));

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
    }
}
