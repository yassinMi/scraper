using scraper.Core;
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
        public Plugin Plugin { get; set; }
        
        
        
        public static Workspace CreateOne(string workspacePath,Plugin plugin)
        {
            if (string.IsNullOrWhiteSpace(workspacePath)) throw new Exception("workspacePath cannot be null");
            if (Exists(workspacePath)) throw new Exception($"Workspace already exists at {workspacePath}");
            //creating the sub directories
            Workspace res = new Workspace() { Directory = workspacePath };
            res.CSVResources = new List<CSVResource>();
            
            SetUpWorkspaceFolders(res);
            var all_file_in_csv = System.IO.Directory.GetFiles(res.CSVOutputFolder);
            //we're still populating the CSVResources from files because data being there does'nt prevent creation of the workspace (aka the plugin ptr file)
            foreach (var item in all_file_in_csv)
            {
                if (Path.GetExtension(item).ToLower().Replace(".", "") == "csv")
                {
                    var o = new CSVResource() { Path = new Uri(item) };
                    o.Check();
                    res.CSVResources.Add(o);
                }
            }
            //creating the plugin file
            File.WriteAllText(Path.Combine(workspacePath, Workspace.PLUGINS_PTR_FILE_RELATIVE_PATH), plugin.Name);
            res.Plugin = plugin;
            return res;
        }
        /// <summary>
        /// incomplete: doesnt loead the Plugin property (utils refactoring needed)
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <returns></returns>
        public static Workspace Load(string workspacePath)
        {
            if (string.IsNullOrWhiteSpace( workspacePath )) throw new Exception("workspacePath cannot be null"); // workspacePath = ConfigService.Instance.WorkspaceDirectory;
            if (!Exists(workspacePath)) throw new Exception($"cannot load workspace at '{workspacePath}' because the're is no folder and/or plugin ptr file");
            Workspace res = new Workspace() { Directory = workspacePath };
            res.CSVResources = new List<CSVResource>();
            
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
        /// Loads and makes current 
        /// currently this only usfull in testing
        /// calling this assigns the Current property a value with the workspace path specified, 
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <returns></returns>
        public static void MakeCurrent(string workspacePath )
        {
            Debug.WriteLine("updating Workspace.current: " + workspacePath);
            _current = Load(workspacePath);
        }

        public static void MakeCurrent(Workspace ws)
        {
            _current = ws;
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

        public IEnumerable<PluginScrapingTask> GetScrapingTasksFromFiles()
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
        /// exceptions: File.Exists()'s
        /// based on PLUGINS_PTR_FILE existance
        /// </summary>
        /// <param name="workingDirectoryInputValue"></param>
        /// <returns></returns>
        public static bool Exists(string ws_path)
        {
            return File.Exists(Path.Combine(ws_path, PLUGINS_PTR_FILE_RELATIVE_PATH));
        }

        internal static string[] GetPluginsNamesUnderWorkspace(Workspace ws)
        {
            string[] res = null;
            try
            {
                string[] pluginsNames = File.ReadAllLines(ws.PluginsPtrFilePath);
                return pluginsNames;
            }
            catch (Exception) { }
            return res;
        }


        public static string PLUGINS_PTR_FILE_RELATIVE_PATH = @".scraper\plugins";
        /// <summary>
        /// text file under .scraper/ that lists the Names of the plugins used in the workdpace (separated by envrements.newLine)
        /// </summary>
        public string PluginsPtrFilePath { get { return Path.Combine(this.Directory,Workspace.PLUGINS_PTR_FILE_RELATIVE_PATH ); } }
        public string ElementsImagesFolder { get { return Path.Combine(this.Directory, Options.ElementsImagesRelativeLocation); } }
        public string[] PluginsNames { get { return GetPluginsNamesUnderWorkspace(this); } }

        

        public string HtmlObjectsFolder { get { return Path.Combine(this.Directory, Options.ElementsHTMLRelativeLocation); } }
        public string CSVOutputFolder { get { return Path.Combine(this.Directory, Options.CSVOutputRelativeLocation);} }
        public string TPFolder { get { return Path.Combine(this.Directory, Options.TargetPagesRelativeLocation); } }
       
        
    }
}
