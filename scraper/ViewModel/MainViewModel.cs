using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mi.Common;
using Microsoft.VisualBasic.FileIO;

using Jitbit.Utils;
using scraper.Model;
using System.Windows.Input;
using System.Diagnostics;
using scraper.Services;
using System.ComponentModel;
using System.Collections.Specialized;
using scraper.Core;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace scraper.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        public MainViewModel(IPlugin plugin, Workspace workspace)
        {
            MainPlugin = plugin;
            MainWorkspace = workspace;
            Init();


        }
        public MainViewModel()
        {
            MainWorkspace = Workspace.Current;

            if (App.Current?.MainWindow != null && DesignerProperties.GetIsInDesignMode(App.Current.MainWindow))
            {
                foreach (var i in (new List<ScrapingTaskVM>{
                    new ScrapingTaskVM(),
                    new ScrapingTaskVM(),
                    new ScrapingTaskVM(),

                }))
                {
                    ScrapingTasksVMS.Add(i);
                }
                //CurrentScrapTask = new ScrapTask(@"https://www.microcenter.com/search/search_results.aspx?N=4294966937&NTK=all&sortby=match&rpp=96&myStore=false&page=2") { Stage = ScrapTaskStage.DownloadingData, DownloadingProgress = new DownloadingProg() { Total = 512, Current = 438 } };
                //CurrentTaskDetail = "lkjk hk jhzlkdj hlkzjehk jhzekj hzekjhk jhkejhk hekjhkejh khkj hkehkj hkehkj hkejh kjehkjeh kj ehke hkj hkejhk hkjhk jehk hekh kejh ekjhk ejhke jhk jhkejh kjhekhkejhk ehk hekj";
            }

            Init();


        }

        private void Init()
        {
            Debug.WriteLine("GetScrapingTasksFromFiles");
            foreach (var p in PluginsManager.GetGlobalPlugins())
            {
                GlobalUserPlugins.Add(p);
            }

            foreach (var i in MainWorkspace.GetScrapingTasksFromFiles())
            {
                Debug.WriteLine(i);

                ScrapingTasksVMS.Add(new ScrapingTaskVM(i));
            }

            CSVResourcesVMS = new ObservableCollection<CSVResourceVM>(MainWorkspace.CSVResources.Select((sr) =>
            {
                return new CSVResourceVM(sr);
            }));
            notif(nameof(CurrentPluginName));

            ScrapingTasksVMS = new ObservableCollection<ScrapingTaskVM>();

        }



        IPlugin MainPlugin;
        Workspace MainWorkspace;
        IEnumerable<BusinessViewModel> BusinessViewModels_arr = new List<BusinessViewModel>();


        private string _CurrentPluginName;//to be extended to more detailed PluginInfo struct
        public string CurrentPluginName
        {

            get { return MainPlugin.Name; }
        }


        public string ElementName
        {
            get { return MainPlugin.ElementName; }
        }

        public string ElementNamePlural
        {
            get { return MainPlugin.ElementNamePlural; }
        }


        public ObservableCollection<IPlugin> GlobalUserPlugins { get; set; } = new ObservableCollection<IPlugin>();

        private async void onDirtyCSVResourceVMSelection()
        {
            //logic that need to be performed when some items changest's IsActive
            TotalRecordsCountString = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, int>(0, (v, i) => v + i.RowsCount).ToString();

            BusinessViewModels_arr = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, IEnumerable<BusinessViewModel>>(new List<BusinessViewModel>(), (i, csvVM) => {
                return i.Concat(Utils.parseCSVfile(csvVM.FullPath).Select(p => new BusinessViewModel(p)));
            });
            BusinessesViewModels = new ObservableCollection<BusinessViewModel>(BusinessViewModels_arr);
            notif(nameof(BusinessesViewModels));
            await Task.Delay(0);


        }
        private void handl_CSVResourceVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                onDirtyCSVResourceVMSelection();
            }
        }

        private void handl_CSVResourceVMS_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in _CSVResourcesVMS)
            {
                item.PropertyChanged += (handl_CSVResourceVM_PropertyChanged);
            }
            onDirtyCSVResourceVMSelection();
        }



        public ObservableCollection<BusinessViewModel> BusinessesViewModels { get {
                return new ObservableCollection<BusinessViewModel>(
                    BusinessViewModels_arr.Where(p => p.Name.ToLower().Contains(SearchQuery.ToLower().Trim())));
            }
            set {
                notif(nameof(BusinessesViewModels));
            } }

        private ObservableCollection<CSVResourceVM> _CSVResourcesVMS = new ObservableCollection<CSVResourceVM>();

        public ObservableCollection<CSVResourceVM> CSVResourcesVMS
        {
            get { return _CSVResourcesVMS; }
            set { _CSVResourcesVMS = value;
                _CSVResourcesVMS.CollectionChanged += handl_CSVResourceVMS_CollectionChanged;
                foreach (var item in _CSVResourcesVMS)
                {
                    item.PropertyChanged += (handl_CSVResourceVM_PropertyChanged);
                }
                onDirtyCSVResourceVMSelection();
            }
        }

        public ObservableCollection<System.Windows.Media.Brush> brushess { get; set; } =
            new ObservableCollection<System.Windows.Media.Brush>(
                new System.Windows.Media.Brush[]
                {System.Windows.Media.Brushes.AliceBlue,
                System.Windows.Media.Brushes.Yellow}
    );


        public MainWindow mw { get; set; } = null;


        public string CurrentWorkspaceDirectory
        {
            get { return MainWorkspace.Directory; }
        }

        public string CurrentWorkspaceName
        {
            get { return Path.GetFileName(MainWorkspace?.Directory ?? "none"); }
        }

        private string _TotalRecordsCountString = "0";
        public string TotalRecordsCountString
        {
            set { _TotalRecordsCountString = value; notif(nameof(TotalRecordsCountString)); }
            get { return _TotalRecordsCountString; }
        }


        private string _SearchQuery = "";
        public string SearchQuery
        {
            set { _SearchQuery = value; notif(nameof(SearchQuery));
                notif(nameof(BusinessesViewModels));
            }
            get { return _SearchQuery; }
        }


        private string _TargetPageQueryText;
        public string TargetPageQueryText
        {
            set { _TargetPageQueryText = value; notif(nameof(TargetPageQueryText)); }
            get { return _TargetPageQueryText; }
        }


        public enum ElementsViewTypes { Grid = 0, List }


        private ObservableCollection<ScrapingTaskVM> _ScrapingTasksVMS;
        public ObservableCollection<ScrapingTaskVM> ScrapingTasksVMS { get {
                return _ScrapingTasksVMS;
            } set {
                _ScrapingTasksVMS = value;
                BindingOperations.EnableCollectionSynchronization(_ScrapingTasksVMS, ScrapingTasksVMS_lock);


            }
        } 



        private ElementsViewTypes _ElementsViewType;
        public ElementsViewTypes ElementsViewType
        {
            set { _ElementsViewType = value; notif(nameof(ElementsViewType)); }
            get { return _ElementsViewType; }
        }


        private async void handleStartScrapingCommand()
        {
            Debug.WriteLine("handleStartScrapingCommand");

            var newScrapTask = MainPlugin.GetTask(TargetPageQueryText);
            Debug.WriteLine("new ScrapingTaskVM");

            var tvm = new ScrapingTaskVM(newScrapTask);
            Debug.WriteLine("nadd");

            
            ScrapingTasksVMS.Add(tvm);
            Debug.WriteLine("stage");


            newScrapTask.Stage = ScrapTaskStage.DownloadingData;
            Debug.WriteLine("RunScraper");

            await newScrapTask.RunScraper();
            newScrapTask.Stage = ScrapTaskStage.ConvertingData;
            await newScrapTask.RunConverter();

            // Debug.WriteLine("return code is: scraper:" + res.ToString()+", converter:"+ res_c.ToString());
        }

        //for testing
        public TaskAwaiter scrapingTaskAwaiter { get; set; }

        public  void callScrapingTask()
        {
            Debug.WriteLine("callScrapingTask");
            Task tsl = new Task(handleStartScrapingCommand);
            Debug.WriteLine("getting awaiter");
            scrapingTaskAwaiter = tsl.GetAwaiter();
            Debug.WriteLine("starting tsll");

            tsl.Start();

        }
        private bool canExecuteStartScrapingCommand()
        {
            /*if (CurrentScrapTaskStage == ScrapTaskStage.DownloadingData) return false;
            if (CurrentScrapTaskStage == ScrapTaskStage.ParsingPage) return false;
            if (CurrentScrapTaskStage == ScrapTaskStage.ConvertingData) return false;
            */
            Uri uri;
            if (Uri.TryCreate(TargetPageQueryText, UriKind.Absolute, out uri) == false)
            {
                return false;
            }
            string website = uri.Host.ToLower();
            if (!((website== "www.businesslist.ph") )) return false;
            return true;
        }
        private ICommand _StartScrapingCommand = null;
        private object ScrapingTasksVMS_lock = new object();

        /// <summary>
        /// starts scraing , takes no argument, only uses the query string
        /// </summary>
        public ICommand StartScrapingCommand { get {
                if(_StartScrapingCommand==null) _StartScrapingCommand = new MICommand(callScrapingTask, canExecuteStartScrapingCommand);

                return _StartScrapingCommand;
            } }

         private void HndlLoadCSVFileCommand()
        {
            Debug.Write("ok csv");
            IOUtils.PromptOpeningPathAsync((string maybeFile,bool canceled) =>
            {
                if (canceled)
                    return;
                var lst = Utils.parseCSVfile(maybeFile).Select((p) =>
                {
                    return new BusinessViewModel(p);
                }).ToList();
                BusinessViewModels_arr = new ObservableCollection<BusinessViewModel> (lst);
                notif(nameof( BusinessesViewModels));
                

            });
        }
        public ICommand LoadCSVFileCommand { get { return new MICommand(HndlLoadCSVFileCommand, ()=>true); } }

        private void handleCloseWindowCommand()
        {
            if(mw!=null)
            mw.Close();
        }
        public ICommand CloseWindowCommand { get { return new MICommand(handleCloseWindowCommand); } }

        private void handleRefreshWorkspaceCommand()
        {
            MainWorkspace.refresh();
            notif(nameof(CurrentWorkspaceDirectory));
            CSVResourcesVMS.Clear();
            foreach (var sr in MainWorkspace.CSVResources)
            {
                CSVResourcesVMS.Add(new CSVResourceVM(sr));
            }

        }
        public ICommand RefreshWorkspaceCommand { get { return new MICommand(handleRefreshWorkspaceCommand); } }



        public ICommand SwitchElementsViewType { get { return new MICommand(hndlSwitchElementsViewType); } }

        private void hndlSwitchElementsViewType()
        {
            this.ElementsViewType = (ElementsViewTypes)  (( ((int)ElementsViewType) + 1 )% Enum.GetNames(typeof(ElementsViewTypes)).Count());
        }
    }
}
