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
using scraper.View;
using scraper.Plugin;
using System.Windows;
using System.Collections;

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
            IsWorkspaceSetupMode = true;
            WorkingDirectoryInputValue = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\MyWorkspace1");
            MainWorkspace = null; 
        }


        /// <summary>
        /// loads workspace nd plugn specific UI elements, cannot be called befor assigning main ws
        /// can be called late after setting up the workspace in runtime
        /// </summary>
        private void Init()
        {
            Trace.Assert(MainWorkspace != null, "Cannot init mainViewModel with MainWorkspace=null");
            Trace.Assert(MainPlugin != null, "Cannot init mainViewModel with MainPlugin=null");

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
            notif(nameof(ElementNamePlural));

        }


       
        IPlugin _MainPlugin;
        IPlugin MainPlugin {
            get { return _MainPlugin; }
            set {
                _MainPlugin = value;
                notif(nameof(MainPlugin));
                notif(nameof(ElementName));
                notif(nameof(ElementNamePlural));
                notif(nameof(CurrentPluginName));
                notif(nameof(PluginHelpVM));

            }
        }

        Workspace _MainWorkspace;
        Workspace MainWorkspace
        {
            get { return _MainWorkspace; }
            set {
                _MainWorkspace = value;
                notif(nameof(MainWorkspace));
                notif(nameof(CurrentWorkspaceDirectory));
                notif(nameof(CurrentWorkspaceName));

            }
        }
        IEnumerable<BusinessViewModel> BusinessViewModels_arr = new List<BusinessViewModel>();


        //to be extended to more detailed PluginInfo struct
        public string CurrentPluginName
        {

            get { return MainPlugin?.Name; }
        }


        public bool _IsWorkspaceSetupMode ;
        public bool IsWorkspaceSetupMode
        {
            set { _IsWorkspaceSetupMode = value; notif(nameof(IsWorkspaceSetupMode)); }
            get { return _IsWorkspaceSetupMode; }
        }


        public string  _WorkingDirectoryInputValue;
        public string WorkingDirectoryInputValue
        {
            set { _WorkingDirectoryInputValue = value; notif(nameof(WorkingDirectoryInputValue)); }
            get { return _WorkingDirectoryInputValue; }
        }




        public HelpVM PluginHelpVM { get
            {
                return new HelpVM(MainPlugin?.UsageInfo);
            } }

        public string ElementName
        {
            get { return MainPlugin?.ElementName; }
        }

        public string ElementNamePlural
        {
            get { return MainPlugin?.ElementNamePlural; }
        }


        public void cleanCheckCSVResorces()
        {
            
            List<CSVResourceVM> tobeRemoved = new List<CSVResourceVM>( CSVResourcesVMS.Where(v =>
            {
                v.recheck();
                return v.IsRemoved;
            }));
            foreach (var item in tobeRemoved)
            {
                CSVResourcesVMS.Remove(item);
            }
        }

        public ObservableCollection<IPlugin> GlobalUserPlugins { get; set; } = new ObservableCollection<IPlugin>();

        private async void onDirtyCSVResourceVMSelection()
        {

            cleanCheckCSVResorces();
            
            //logic that need to be performed when some items changest's IsActive
            TotalRecordsCountString = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, int>(0, (v, i) => v + i.RowsCount).ToString();

            BusinessViewModels_arr = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, IEnumerable<BusinessViewModel>>(new List<BusinessViewModel>(), (i, csvVM) => {
                var enumerated = Utils.parseCSVfile(csvVM.FullPath);
                if (enumerated == null) return i;
                return i.Concat(enumerated.Select(p => new BusinessViewModel(p)));
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


        private ObservableCollection<BusinessViewModel>  _GridResults = new ObservableCollection<BusinessViewModel>();

        public ObservableCollection<BusinessViewModel>  GridResults
        {
            set { _GridResults = value; notif(nameof(GridResults)); }
    get { return _GridResults ; }
         
        }



        private int _SelectionCount;
        public int SelectionCount
        {
            set { _SelectionCount = value; notif(nameof(SelectionCount)); }
            get { return _SelectionCount; }
        }



        /// <summary>
        /// temp workaround
        /// </summary>
        public void UpdateDataGridStats()
        {
            
        }



        public ObservableCollection<BusinessViewModel> BusinessesViewModels { get {
                if(string.IsNullOrWhiteSpace(SearchQuery)) return new ObservableCollection<BusinessViewModel>(
                    BusinessViewModels_arr);
            
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
                notif(nameof(CSVResourcesVMS));
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
            get { return MainWorkspace?.Directory; }
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
                notif(nameof(ScrapingTasksVMS));

            }
        } 



        private ElementsViewTypes _ElementsViewType;
        public ElementsViewTypes ElementsViewType
        {
            set { _ElementsViewType = value; notif(nameof(ElementsViewType)); }
            get { return _ElementsViewType; }
        }


        private bool _IsHelpPopupOpen;
        public bool IsHelpPopupOpen
        {
            set { _IsHelpPopupOpen = value; notif(nameof(IsHelpPopupOpen)); }
            get { return _IsHelpPopupOpen; }
        }




        private async void handleStartScrapingCommand()
        {
            Trace.Assert(MainWorkspace != null, "cannot start task with MainWorkspace=null");
            Trace.Assert(MainPlugin != null, "cannot start task with main plugin=null");
            Debug.WriteLine("handleStartScrapingCommand");

            var newScrapTask = MainPlugin.GetTask(TargetPageQueryText);
            Debug.WriteLine("new ScrapingTaskVM");

            var tvm = new ScrapingTaskVM(newScrapTask);
            Debug.WriteLine("nadd");

            
            ScrapingTasksVMS.Add(tvm);
            Debug.WriteLine("stage");


            
            Debug.WriteLine("RunScraper");
            
            await newScrapTask.RunScraper();
            

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
            Trace.Assert(MainWorkspace != null, "Cannot handleRefreshWorkspaceCommand with MainWorkspace=null");
            MainWorkspace.refresh();
            notif(nameof(CurrentWorkspaceDirectory));
            Debug.WriteLine("clearing resources");
            CSVResourcesVMS.Clear();
            foreach (var sr in MainWorkspace.CSVResources)
            {
                Debug.WriteLine("adding resource");
                CSVResourcesVMS.Add(new CSVResourceVM(sr));
            }

        }
        public ICommand RefreshWorkspaceCommand { get { return new MICommand(handleRefreshWorkspaceCommand); } }



        public ICommand SwitchElementsViewType { get { return new MICommand(hndlSwitchElementsViewType); } }

        private void hndlSwitchElementsViewType()
        {
            this.ElementsViewType = (ElementsViewTypes)  (( ((int)ElementsViewType) + 1 )% Enum.GetNames(typeof(ElementsViewTypes)).Count());
        }


        public ICommand OpenWorkspaceCommand { get
            {
                return new MICommand<string>(hndlOpenWorkspaceCommand);
            } }

        private void hndlOpenWorkspaceCommand(string ws_dir)
        {
            if (!Directory.Exists(ws_dir))
            {
                try
                {
                    Directory.CreateDirectory(ws_dir);
                }
                catch (Exception)
                {
                    
                    return;
                }
                
            }
            Workspace.MakeCurrent(ws_dir);
            var ws = Workspace.Current;
            IPlugin p = new BLScraper() { WorkspaceDirectory = ws.Directory };
            MainWorkspace = ws;
            MainPlugin = p;
            Init();
            if (IsWorkspaceSetupMode == true)
            {
                IsWorkspaceSetupMode = false;
            }
            ConfigService.Instance.WorkspaceDirectory = ws.Directory;
            ConfigService.Instance.Save();
        }

        public ICommand OpenPluginOptionsCommand { get { return new MICommand(hndlOpenPluginOptionsCommand); } }

        private void hndlOpenPluginOptionsCommand()
        {
            PluginOptionsWindow pow = new PluginOptionsWindow();
            pow.ShowDialog();
        }

        public ICommand OpenHelpPopupCommand { get { return new MICommand(hndlOpenHelpPopupCommand); } }

        private void hndlOpenHelpPopupCommand()
        {
            IsHelpPopupOpen = true;
        }

        public ICommand DevFillAndStartCommand { get { return new MICommand<string>(hndlDevFillAndStartCommand); } }

        private void hndlDevFillAndStartCommand(string variant)
        {
            TargetPageQueryText = variant == "4" ? @"https://www.businesslist.ph/category/industrial-premises" 
            : variant == "9" ? @"https://www.businesslist.ph/location/santa-rosa-city"
            : "";
            StartScrapingCommand.Execute(null);
        }

        public ICommand SaveResultsCommand { get { return new MICommand(hndlSaveResultsCommand, canExecuteSaveResultsCommand); } }

        private bool canExecuteSaveResultsCommand()
        {
            return BusinessesViewModels.Count > 0;
        }

        private void hndlSaveResultsCommand()
        {
            
            IOUtils.PromptSavingPathAsync(".csv", (s, canceled) =>
            {
                if (!canceled)
                {
                    Utils.CSVOverwriteRecords(s, BusinessesViewModels.Select(vm => vm.Model));
                    MessageBox.Show($"Saved to s{s}","Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            },"Save results","CSV File|*.csv|All Files|*");
        }

        public ICommand SaveSelectionCommand { get { return new MICommand(hndlSaveSelectionCommand, canExecuteSaveSelectionCommand); } }

        public IEnumerable<BusinessViewModel> DataGridSelectedItemsRef { get; internal set; }

        private bool canExecuteSaveSelectionCommand()
        {
            return SelectionCount > 0;
        }

        private void hndlSaveSelectionCommand()
        {
            IOUtils.PromptSavingPathAsync(".csv", (s, canceled) =>
            {
                if (!canceled)
                {

                    if((DataGridSelectedItemsRef?.Any() ??false)!=false)
                    {
                        Utils.CSVOverwriteRecords(s, (DataGridSelectedItemsRef as IEnumerable<BusinessViewModel>)?.Select(vm => vm.Model));
                        MessageBox.Show($"Saved to s{s}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    }

                }
                else
                {
                    MessageBox.Show("ca");
                }

            }, "Save selection", "CSV File|*.csv|All Files|*");
        }

        public ICommand PickWorkspaceFolderCommand { get { return new MICommand(hndlPickWorkspaceFolderCommand); } }

        private void hndlPickWorkspaceFolderCommand()
        {
            IOUtils.PromptPickingDirectory((s, canceled) =>
            {
                this.WorkingDirectoryInputValue = s;
            }, "Chose working directory");
        }
    }
}
