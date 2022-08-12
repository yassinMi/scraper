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
using scraper.Core.Workspace;
using scraper.Core.Utils;
using scraper.ViewModel.HomeScreen;
using System.Threading;
using System.Windows.Threading;

namespace scraper.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        public MainViewModel(Core.Plugin plugin, Workspace workspace)
        {
            if (plugin == null) throw new ArgumentNullException( nameof(plugin), "cannot create MainViewModel instance using null plugin param");
            if (plugin == null) throw new ArgumentNullException(nameof(plugin), "cannot create MainViewModel instance using null workspace param");
            MainPlugin = plugin;
            MainWorkspace = workspace;
            Init();
        }
        public MainViewModel()
        {
            int i = 1;
            string new_suggested_ws_path = "";
            while (System.IO.Directory.Exists(new_suggested_ws_path = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\MyWorkspace{i}"))) i++;
            IsWorkspaceSetupMode = true;
            WorkingDirectoryInputValue = (new_suggested_ws_path);
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

            foreach (var p in PluginsManager.CachedGlobalPlugins)
            {
                GlobalUserPlugins.Add(p);

            }
            foreach (var i in MainWorkspace.GetScrapingTasksFromFiles())
            {
                Debug.WriteLine("adding ScrapingTasksVMS");
                Debug.WriteLine(i);
                ScrapingTasksVMS.Add(new ScrapingTaskVM(i));
            }

            CSVResourcesVMS = new ObservableCollection<CSVResourceVM>(MainWorkspace.CSVResources.Select((sr) =>
            {

                return new CSVResourceVM(sr, MainPlugin);
            }));
            notif(nameof(CurrentPluginName));

            ScrapingTasksVMS = new ObservableCollection<ScrapingTaskVM>();
            notif(nameof(ElementNamePlural));
            //ElementFieldsNamesInFilterPanel = MainPlugin.ElementDescription.Fields.Select(f=>f.UIName); //todo after lossless refactoring 

            FilterRulesVMS = new ObservableCollection<FilteringRuleViewModel>();
            ElementFields = MainPlugin.ElementDescription.Fields.Select(f => f).ToArray();
            ElementFieldsNames = MainPlugin.ElementDescription.Fields.Select(f => f.UIName).ToArray();

            FilterComponenetsVMS = new ObservableCollection<BaseFilterComponentViewModel>();
            var FilterComponentDescriptors = MainPlugin.FiltersDescription;
            if (FilterComponentDescriptors != null)
            {
                foreach (var desc in FilterComponentDescriptors)
                {
                    if(desc.Type== FilterComponenetType.GroupFilter)
                    {
                        var gfc_model = new GroupsFilterComponentModel(MainPlugin.ElementModelType, desc.PropertyName,desc.Header);
                        var vm = new GroupsFilterComponentViewModel(gfc_model);
                        gfc_model.GroupsSelectionGotDirty += (s, e) =>
                        {
                            Debug.WriteLine("GroupsSelectionGotDirty");
                            notif(nameof(ElementsViewModels));
                        };
                        vm.Model.Update(ElemenetsVMSLoaded.Select(m => m.Model));
                        FilterComponenetsVMS.Add(vm);
                        Debug.WriteLine("added  FilterComponenetsVMS item");
                    }
                    else if (desc.Type == FilterComponenetType.RangeFilter)
                    {
                        var rfc_model = new RangeFilterComponentModel( desc);
                        var vm = new RangeFilterComponentViewModel(rfc_model);
                        rfc_model.RangeParamsGotDirty += (s, e) =>
                        {
                            Debug.WriteLine("RangeParamsGotDirty");
                            notif(nameof(ElementsViewModels));
                        };
                        vm.Model.Update(ElemenetsVMSLoaded.Select(m => m.Model));
                        FilterComponenetsVMS.Add(vm);
                        Debug.WriteLine("added  FilterComponenetsVMS item");
                    }
                }
                 }
           
            
            Debug.WriteLine("endof init");

        }



        Core.Plugin _MainPlugin;
        Core.Plugin MainPlugin {
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

        private List<ElementViewModel> _ElemenetsVMSLoaded = new List<ElementViewModel>();
        /// <summary>
        /// the loaded data unfiltered, this contains all elements from the currently enabled csv resources.
        /// </summary>
        List<ElementViewModel> ElemenetsVMSLoaded { get {
                return _ElemenetsVMSLoaded;
            } set
            {
                _ElemenetsVMSLoaded = value;
                onElemenetsVMSLoadedChanged();
            } }

        private void onElemenetsVMSLoadedChanged()
        {
            if(FilterComponenetsVMS!=null)
            foreach (var C in FilterComponenetsVMS)
            {
                C.Model.Update(ElemenetsVMSLoaded.Select(m => m.Model)); //todo improve by removing select 
            }
        }

        private IEnumerable<string> _ElementFieldsNames;
        public IEnumerable<string> ElementFieldsNames
        {
            set { _ElementFieldsNames = value; notif(nameof(ElementFieldsNames)); }
            get { return _ElementFieldsNames; }
        }

        private IEnumerable<Field> _ElementFields;
        public IEnumerable<Field> ElementFields
        {
            set { _ElementFields = value; notif(nameof(ElementFields)); }
            get { return _ElementFields; }
        }


        public IEnumerable<string> FilterRuleTypesNames { get; set; } = Enum.GetNames(typeof(FilteringRuleType));




        public string WindowTitle
        {

            get
            {
                return
                   (MainPlugin?.TargetHost != null) ? $"scraper v{ApplicationInfo.APP_VERSION} - {MainPlugin.TargetHost}"
                  : (MainPlugin?.Name != null) ? $"scraper v{ApplicationInfo.APP_VERSION} - {MainPlugin.Name}"
                  : $"scraper v{ApplicationInfo.APP_VERSION}";
            }
        }

        //to be extended to more detailed PluginInfo struct
        public string CurrentPluginName
        {

            get { return MainPlugin?.Name; }
        }

        public string CurrentPluginTargetHost
        {

            get { return MainPlugin?.TargetHost; }
        }


        public IEnumerable<RecentWorkspaceVM> RecentlyOpenedWorkspaces { get {
                return ConfigService.Instance.RecentWorkspaces.Select(p => new RecentWorkspaceVM(p) { mainViewModelRef=this});
            } }

        public IEnumerable<Core.Plugin> AllInstalledPlugins
        {
            get
            {
                return PluginsManager.CachedGlobalPlugins;
            }
        }

        public IEnumerable<Core.Plugin> AllPluginPickerPlugins
        {
            get
            {
                return FilePickerPlugin==null? AllInstalledPlugins : AllInstalledPlugins.Concat(new Core.Plugin[] { FilePickerPlugin });
            }
        }

        public Core.Plugin FilePickerPlugin { get; set; } = null;

        private FilteringRuleType _CurrentFilteringRuleTypeInput;
        public string CurrentFilteringRuleTypeInput
        {
            set {
                if(value is string)
                {
                    _CurrentFilteringRuleTypeInput = (FilteringRuleType) Enum.Parse(typeof(FilteringRuleType), value);
                }
                
                notif(nameof(CurrentFilteringRuleTypeInput));
                notif(nameof(IsCurrentFilteringRuleParamInputVisible));
            }
            get { return Enum.GetName(typeof(FilteringRuleType), _CurrentFilteringRuleTypeInput) ; }
        }


        
        public bool IsCurrentFilteringRuleParamInputVisible
        {
            get { return _CurrentFilteringRuleTypeInput!= FilteringRuleType.IsNotEmpty; }
        }



        private Field _CurrentFilteringRuleFiedlInput;
        public Field CurrentFilteringRuleFiedlInput
        {
            set { _CurrentFilteringRuleFiedlInput = value;
                notif(nameof(CurrentFilteringRuleFiedlInput)); }
            get { return _CurrentFilteringRuleFiedlInput; }
        }


        private string _CurrentFilteringRuleParamInput;
        public string CurrentFilteringRuleParamInput
        {
            set { _CurrentFilteringRuleParamInput = value; notif(nameof(CurrentFilteringRuleParamInput)); }
            get { return _CurrentFilteringRuleParamInput; }
        }





        public bool _IsWorkspaceSetupMode ;
        public bool IsWorkspaceSetupMode
        {
            set { _IsWorkspaceSetupMode = value; notif(nameof(IsWorkspaceSetupMode)); }
            get { return _IsWorkspaceSetupMode; }
        }

        /// <summary>
        /// when IsWorkspaceSetupMode, this siply indicates whether the current directory points to an existing workspace (false), based the plugin pointer file existence
        /// </summary>
        public bool IsCreateModeOrOpenMode
        {
            get {
                bool existentWorkspace = false;
                try
                {
                    existentWorkspace = Workspace.Exists(WorkingDirectoryInputValue);
                }
                catch (Exception)
                {

                }
                return !existentWorkspace;
            }
        }




        public string  _WorkingDirectoryInputValue;
        public string WorkingDirectoryInputValue
        {
            set
            {   _WorkingDirectoryInputValue = value;
                notif(nameof(WorkingDirectoryInputValue));
                onWorkingDirectoryInputValueChange();
            }
            get { return _WorkingDirectoryInputValue; }
        }

        private void onWorkingDirectoryInputValueChange()
        {
            notif(nameof(IsCreateModeOrOpenMode));
            if (IsCreateModeOrOpenMode==false)
            {
                var firstPluginName = Workspace.GetPluginsNamesUnderWorkspace(WorkingDirectoryInputValue)?.FirstOrDefault();
                if (firstPluginName == null) return;
                var Plugin = PluginsManager.CachedGlobalPlugins.FirstOrDefault(p => p.Name == firstPluginName);
                if (Plugin == null) return;
                PluginPickerInputValue = Plugin;
            }
            else
            {
                PluginPickerInputValue = PluginsManager.CachedGlobalPlugins.FirstOrDefault();
            }
        }


        



        private Core.Plugin _PluginPickerInputValue = PluginsManager.CachedGlobalPlugins.FirstOrDefault();
        public Core.Plugin PluginPickerInputValue
        {
            set { _PluginPickerInputValue = value; notif(nameof(PluginPickerInputValue)); }
            get { return _PluginPickerInputValue; }
        }



        private ObservableCollection<FilteringRuleViewModel> _FilterRulesVMS = null;
        public ObservableCollection<FilteringRuleViewModel> FilterRulesVMS { get
            {
                return _FilterRulesVMS;
            }
            set
            {
                _FilterRulesVMS = value;
                FilterRulesVMS.CollectionChanged += hndlFilterRulesVMSCollectionChanged;
                foreach (var item in FilterRulesVMS)
                {
                    item.OnDeleteRequest += handelFilterRulesVMSDeleteRequest;
                }
                notif(nameof(FilterRulesVMS));
            }
        }

        public void handelFilterRulesVMSDeleteRequest(object s, EventArgs e)
        {
            FilteringRuleViewModel it = s as FilteringRuleViewModel;
            if (it != null)
            {
                FilterRulesVMS.Remove(it);
            }
            
        }

        private void hndlFilterRulesVMSCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ////trigger refiltering
            if(e.NewItems!=null)
            foreach (var item in e.NewItems)
            {

                    FilteringRuleViewModel it = item as FilteringRuleViewModel;
                    if (it != null)
                    {
                        it.OnDeleteRequest += handelFilterRulesVMSDeleteRequest;
                    }
            }
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {

                    FilteringRuleViewModel it = item as FilteringRuleViewModel;
                    if (it != null)
                    {
                        it.OnDeleteRequest -= handelFilterRulesVMSDeleteRequest;
                    }
                }

            notif(nameof(ElementsViewModels));

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


        private string _CurrentTaskDetail;
        public string CurrentTaskDetail
        {
            set { _CurrentTaskDetail = value; notif(nameof(CurrentTaskDetail)); }
            get { return _CurrentTaskDetail; }
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

        public ObservableCollection<Core.Plugin> GlobalUserPlugins { get; set; } = new ObservableCollection<Core.Plugin>();

        private void onDirtyCSVResourceVMSelection()
        {
            Debug.WriteLine("onDirtyCSVResourceVMSelection");
            cleanCheckCSVResorces();
            
            //logic that need to be performed when some items changest's IsActive
            TotalRecordsCountString = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, int>(0, (v, i) => v + i.RowsCount).ToString();

            ElemenetsVMSLoaded = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, IEnumerable<ElementViewModel>>(new List<ElementViewModel>(), (i, csvVM) => {
                var enumerated = CSVUtils.parseCSVfile(MainPlugin.ElementModelType, csvVM.FullPath) .Cast<dynamic>();//ufr
                if (enumerated == null) return i;
                return i.Concat(enumerated.Select(p => new ElementViewModel(p,MainPlugin.ElementDescription)));
            }).ToList();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                            new Action(() =>
                            {
                                ElementsViewModels = new ObservableCollection<ElementViewModel>(ElemenetsVMSLoaded);
                                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                    new Action(() =>
                                    {
                                        notif(nameof(ElementsViewModels));
                                        Debug.WriteLine("GC collect");
                                        GC.Collect(4, GCCollectionMode.Forced);
                                    }));

                            }));
            

            


        }
        private void handl_CSVResourceVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                     DispatcherPriority.ApplicationIdle,
                    new Action(onDirtyCSVResourceVMSelection)
                   )
                ;
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


        private ObservableCollection<ElementViewModel>  _GridResults = new ObservableCollection<ElementViewModel>();

        public ObservableCollection<ElementViewModel>  GridResults
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


        /// <summary>
        /// the final filtered collection that is rendered on the UI, this used both for the List View and Grid View itemsSource, in GridView the Model property is used to access the fields, while in List View the properties exposed through the ElementViewModel class are used instead
        /// </summary>
        public IEnumerable<ElementViewModel> ElementsViewModels { get {
                if(string.IsNullOrWhiteSpace(SearchQuery)) return
                    ElemenetsVMSLoaded
                    .Where(p => FilterRulesVMS.All(r => r.Model.Check(p.Model)))
                    .Where(p => FilterComponenetsVMS.All(c => (c == null || c.Model.Passes(p.Model))))
                ;
                string lowertrim = SearchQuery.ToLower().Trim();
                return 
                    ElemenetsVMSLoaded
                    .Where(p => p.Name.ToLower().Contains(lowertrim))
                    .Where(p => FilterRulesVMS.All(r => r.Model.Check(p.Model)))
                    .Where(p => FilterComponenetsVMS.All(c=>(c == null || c.Model.Passes(p.Model))))
                    ;
                }
            set {
                notif(nameof(ElementsViewModels));
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


        private int _SelectedAppTabIndex=0;
        public int SelectedAppTabIndex
        {
            set { _SelectedAppTabIndex = value; notif(nameof(SelectedAppTabIndex)); }
            get { return _SelectedAppTabIndex; }
        }



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
                notif(nameof(ElementsViewModels));
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



        private bool _IsCreatingFilterRuleDlgOpen;
        public bool IsCreatingFilterRuleDlgOpen
        {
            set { _IsCreatingFilterRuleDlgOpen = value; notif(nameof(IsCreatingFilterRuleDlgOpen)); }
            get { return _IsCreatingFilterRuleDlgOpen; }
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


        private ObservableCollection<BaseFilterComponentViewModel> _FilterComponenetsVMS;
        public ObservableCollection<BaseFilterComponentViewModel> FilterComponenetsVMS
        {
            set { _FilterComponenetsVMS = value; notif(nameof(FilterComponenetsVMS)); }
            get { return _FilterComponenetsVMS; }
        }


        private  GroupsFilterComponentViewModel _DevGroupsFilterVM = null;
        public GroupsFilterComponentViewModel DevGroupsFilterVM
        {
            set { _DevGroupsFilterVM = value; notif(nameof(DevGroupsFilterVM)); }
            get { return _DevGroupsFilterVM; }
        }




        private async void handleStartScrapingCommand()
        {
            Trace.Assert(MainWorkspace != null, "cannot start task with MainWorkspace=null");
            Trace.Assert(MainPlugin != null, "cannot start task with main plugin=null");
            Debug.WriteLine("handleStartScrapingCommand");

            var newScrapTask = MainPlugin.GetTask(TargetPageQueryText);
            newScrapTask.TaskDetailChanged += (s, d) =>
            {
                CurrentTaskDetail = d;
            };
            Debug.WriteLine("new ScrapingTaskVM");

            var tvm = new ScrapingTaskVM(newScrapTask);
            Debug.WriteLine("nadd");

            
            ScrapingTasksVMS.Add(tvm);
            Debug.WriteLine("stage");


            
            Debug.WriteLine("RunScraper");
            tvm.currentCTS = new System.Threading.CancellationTokenSource();
            await newScrapTask.RunScraper(tvm.currentCTS.Token);
            

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
            return MainPlugin != null && MainPlugin.ValidateTargetPageInputQuery(TargetPageQueryText);
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
            var lst = CSVUtils.parseCSVfile(MainPlugin.ElementModelType, maybeFile).Cast<dynamic>().Select((p) => //ufr
                {
                    return new ElementViewModel(p,MainPlugin.ElementDescription);
                }).ToList();
                ElemenetsVMSLoaded = new List<ElementViewModel> (lst);
                notif(nameof( ElementsViewModels));
                

            });
        }
        public ICommand LoadCSVFileCommand { get { return new MICommand(HndlLoadCSVFileCommand, ()=>true); } }

        private void handleCloseWindowCommand()
        {
            if (mw!=null)
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
                CSVResourcesVMS.Add(new CSVResourceVM(sr, MainPlugin));
            }

        }
        public ICommand RefreshWorkspaceCommand { get { return new MICommand(handleRefreshWorkspaceCommand); } }



        public ICommand SwitchElementsViewType { get { return new MICommand(hndlSwitchElementsViewType); } }

        private void hndlSwitchElementsViewType()
        {
            this.ElementsViewType = (ElementsViewTypes)  (( ((int)ElementsViewType) + 1 )% Enum.GetNames(typeof(ElementsViewTypes)).Count());
        }


        


        public ICommand OpenRecentWorkspaceCommand { get { return new MICommand<string>(hndlOpenRecentWorkspaceCommand, canExecuteOpenRecentWorkspaceCommand); } }

        private bool canExecuteOpenRecentWorkspaceCommand(string arg)
        {
            return true;
        }

        private void hndlOpenRecentWorkspaceCommand(string ws_dir)
        {
            Workspace ws = null;
            try
            {
                ws = Workspace.Load(ws_dir);
            }
            catch (Exception err) when (err is MissingPluginException || err is WorkspaceNotFoundException)
            {
                MessageBox.Show(err.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Workspace.MakeCurrent(ws);
            Trace.Assert(ws.Plugin != null, "failed to load any plugins, make sure to have a .scraper/plugins file pointing to existing global plugins or chose a plugin before creating workspace");
            MainWorkspace = ws;
            MainPlugin = ws.Plugin;
            Init();
            ClearUI();

            if (IsWorkspaceSetupMode == true)
            {
                IsWorkspaceSetupMode = false;
            }
            ConfigService.Instance.WorkspaceDirectory = ws.Directory;
            Utils.CollectionShift(5, ConfigService.Instance.RecentWorkspaces, new RecentWorkspace() { Path = ws.Directory });
            ConfigService.Instance.RecentWorkspaces = new ObservableCollection<RecentWorkspace>(ConfigService.Instance.RecentWorkspaces.OrderByDescending(rw => rw.IsPinned));
            notif(nameof(RecentlyOpenedWorkspaces));
            ConfigService.Instance.Save();
        }
        /// <summary>
        /// must be called upon switching workspaces to clear dirty UI elemtns, (e,g the filter inputs, search query and the current selected tab )
        /// </summary>
        private void ClearUI()
        {
            Debug.WriteLine("clearing dirty UI");
            SelectedAppTabIndex = 0;
            FilterRulesVMS.Clear();
            FilterRulesVMS = new ObservableCollection<FilteringRuleViewModel>();
            CurrentFilteringRuleFiedlInput = ElementFields.FirstOrDefault();
            CurrentFilteringRuleParamInput = null;
            SearchQuery = null;
            TargetPageQueryText = null;
        }

        public ICommand OpenWorkspaceCommand { get
            {
                return new MICommand<string>(hndlOpenWorkspaceCommand, canExecuteOpenWorkspaceCommand);
            } }

        private bool canExecuteOpenWorkspaceCommand(string arg)
        {
            bool noExceptions = true;
            try
            {
                Path.GetFullPath(arg);
            }
            catch (Exception) { noExceptions = false; }

            return  noExceptions && Path.IsPathRooted(arg);
        }

        /// <summary>
        /// todo: should be renamed as OpenOrCreate or be split into two separate commands
        /// </summary>
        /// <param name="ws_dir"></param>
        private void hndlOpenWorkspaceCommand(string ws_dir)
        {
            Workspace ws;
            if (IsCreateModeOrOpenMode)
            {
                //create mode
                ws = Workspace.CreateOne(ws_dir, PluginPickerInputValue);
                Workspace.MakeCurrent(ws);

            }
            else
            {
                //open mode
                try
                {
                    ws = Workspace.Load(ws_dir);
                }
                catch (MissingPluginException err)
                {
                    MessageBox.Show(err.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return ;
                }
                //ws.Plugin = PluginsManager.CachedGlobalPlugins.FirstOrDefault(pl => pl.Name == ws.PluginsNames.FirstOrDefault());
                
                Workspace.MakeCurrent(ws);
            }
            
            
            
            Trace.Assert(ws.Plugin != null, "failed to load any plugins, make sure to have a .scraper/plugins file pointing to existing global plugins or chose a plugin before creating workspace");
            MainWorkspace = ws;
            MainPlugin = ws.Plugin;
            Init();
            ClearUI();
            if (IsWorkspaceSetupMode == true)
            {
                IsWorkspaceSetupMode = false;
            }
            ConfigService.Instance.WorkspaceDirectory = ws.Directory;
            Utils.CollectionShift<RecentWorkspace>(5, ConfigService.Instance.RecentWorkspaces, new RecentWorkspace() { Path = ws.Directory });
            ConfigService.Instance.RecentWorkspaces = new ObservableCollection<RecentWorkspace>(ConfigService.Instance.RecentWorkspaces.OrderByDescending(rw => rw.IsPinned));
            ConfigService.Instance.Save();
            notif(nameof(RecentlyOpenedWorkspaces));

        }


        public ICommand RecentWorkspacePinToggleCommand { get { return new MICommand<RecentWorkspace>(hndlRecentWorkspacePinToggleCommand, canExecuteRecentWorkspacePinToggleCommand); } }

        private bool canExecuteRecentWorkspacePinToggleCommand(RecentWorkspace arg)
        {
            return true;
        }

        private void hndlRecentWorkspacePinToggleCommand(RecentWorkspace arg)
        {
            int ix = ConfigService.Instance.RecentWorkspaces.IndexOf( arg);
            if (ix == -1) return;
            ConfigService.Instance.RecentWorkspaces[ix] = new RecentWorkspace() { Path = arg.Path, IsPinned = !arg.IsPinned };
            ConfigService.Instance.RecentWorkspaces = new ObservableCollection<RecentWorkspace>(ConfigService.Instance.RecentWorkspaces.OrderByDescending(rw => rw.IsPinned));
            ConfigService.Instance.Save();
            notif(nameof(RecentlyOpenedWorkspaces));
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
            //TargetPageQueryText = variant == "4" ? @"https://www.businesslist.ph/category/industrial-premises" 
            //: variant == "9" ? @"https://www.businesslist.ph/location/santa-rosa-city"
            TargetPageQueryText = variant == "4" ? @"https://www.google.com/search?q=best+software+testing+tools&rlz=1C1CHWL_enMA1011MA1011&oq=bes&aqs=chrome.0.69i59j69i57j46i67i199i465j0i512l4j69i65.2534j0j7&sourceid=chrome&ie=UTF-8"
            : variant == "9" ? @"file:///F:/epicMyth-tmp-6-2022/freelancing/projects/google-results-scraping/best%20software%20testing%20tools2%20-%20Google%20Search.html"
            : "";
            StartScrapingCommand.Execute(null);
        }

        public ICommand SaveResultsCommand { get { return new MICommand(hndlSaveResultsCommand, canExecuteSaveResultsCommand); } }

        private bool canExecuteSaveResultsCommand()
        {
            return ElementsViewModels.Count() > 0;
        }

        private void hndlSaveResultsCommand()
        {
            
            IOUtils.PromptSavingPathAsync(".csv", (s, canceled) =>
            {
                if (!canceled)
                {
                    CSVUtils.CSVOverwriteRecords(s, ElementsViewModels.Select(vm => vm.Model));
                    MessageBox.Show($"Saved to s{s}","Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            },"Save results","CSV File|*.csv|All Files|*");
        }

        public ICommand SaveSelectionCommand { get { return new MICommand(hndlSaveSelectionCommand, canExecuteSaveSelectionCommand); } }

        public IEnumerable<ElementViewModel> DataGridSelectedItemsRef { get; internal set; }

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
                        CSVUtils.CSVOverwriteRecords(s, (DataGridSelectedItemsRef as IEnumerable<ElementViewModel>)?.Select(vm => vm.Model));
                        MessageBox.Show($"Saved to s{s}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    }

                }
                else
                {
                    MessageBox.Show("ca");
                }

            }, "Save selection", "CSV File|*.csv|All Files|*");
        }

        public ICommand PickPluginFileCommand { get { return new MICommand(hndlPickPluginFileCommand,()=>IsCreateModeOrOpenMode); } }

        private void hndlPickPluginFileCommand()
        {
            IOUtils.PromptOpeningPathAsync((s, canceled) =>
            {
                if (canceled) return;
                FilePickerPlugin=PluginsManager.TryLoadFromFile(s) ;
                notif(nameof(AllPluginPickerPlugins));
                if (FilePickerPlugin != null)
                {
                    PluginPickerInputValue = FilePickerPlugin;
                }

            },".dll","Open plugin", "DLL File | *.dll | All Files | *");
        }

        public ICommand PickWorkspaceFolderCommand { get { return new MICommand(hndlPickWorkspaceFolderCommand); } }

        private void hndlPickWorkspaceFolderCommand()
        {
            IOUtils.PromptPickingDirectory((s, canceled) =>
            {
                this.WorkingDirectoryInputValue = s;
            }, "Chose working directory");
        }

        public ICommand AddFilterRuleCommand { get { return new MICommand(hndlAddFilterRuleCommand); } }

        private void hndlAddFilterRuleCommand()
        {
            var newFilteringRuleViewModel = new FilteringRuleViewModel(new FilteringRule()
            {
                fieldName = CurrentFilteringRuleFiedlInput.Name,
                RuleTtype = _CurrentFilteringRuleTypeInput,
                RuleParam = CurrentFilteringRuleParamInput
            });
            
            FilterRulesVMS.Add(newFilteringRuleViewModel);
            IsCreatingFilterRuleDlgOpen = false;
        }

        public ICommand ShowCreatingFilterRuleDlgCommand { get { return new MICommand(hndShowCreatingFilterRuleDlgCommand); } }

        private void hndShowCreatingFilterRuleDlgCommand()
        {
            IsCreatingFilterRuleDlgOpen = true;
        }

        public ICommand CancelCreatingFilterRuleDlgCommand { get { return new MICommand(hndCancelCreatingFilterRuleDlgCommand); } }

        private void hndCancelCreatingFilterRuleDlgCommand()
        {
            IsCreatingFilterRuleDlgOpen = false;

        }

        public ICommand ExitWorkspaceCommand { get { return new MICommand(hndlExitWorkspaceCommand); } }

        private void hndlExitWorkspaceCommand()
        {
            if(!ScrapingTasksVMS.All(s=>s.CurrentScrapTaskStage!= ScrapTaskStage.DownloadingData))
            {
                MessageBox.Show("There are ongoing tasks please stop them or wait untill they complete.");
                return;
            }
            ConfigService.Instance.WorkspaceDirectory = null;
            ConfigService.Instance.Save();
            
            this.MainWorkspace = null;
            this.IsWorkspaceSetupMode = true;
            WorkingDirectoryInputValue = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\MyWorkspace1");

        }
        object reff;

        public ICommand DevGPCommand { get { return new MICommand(hndlDevGPCommand); } }

        private void hndlDevGPCommand()
        {


            return;
            CoreUtils.se();
            return;
            Debug.WriteLine($"creating element item 10000 times into a list");
            List<object> lst = new List<object>();
            foreach (var item in Enumerable.Range(0,1000000))
            {
                dynamic obj = Activator.CreateInstance(MainPlugin.ElementModelType);
                obj.company = $"a long {item} company name";
               
                
                lst.Add (obj);
            }
            dynamic m = lst[0];

            int totlaTextBytes = m.company.Length;
                //+ m.employees.Length + m.address.Length 
                //+ m.contactPerson.Length + m.phonenumber.Length + m.link.Length + m.email.Length;
            MessageBox.Show($"the total text length is {totlaTextBytes}");
            reff = lst;
        }


    }
}
