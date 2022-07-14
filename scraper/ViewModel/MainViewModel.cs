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

namespace scraper.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        public MainViewModel(IPlugin plugin, Workspace workspace)
        {
            MainPlugin = plugin;
            MainWorkspace = workspace;

        }
        public MainViewModel()
        {
            MainWorkspace = Workspace.Current;
            
            if(App.Current?.MainWindow!=null && DesignerProperties.GetIsInDesignMode(App.Current.MainWindow))
            {
                foreach(var i in (new List<ScrapingTaskVM>{
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
           
            Debug.WriteLine("GetScrapingTasksFromFiles");

            foreach (var i in  Workspace.Current.GetScrapingTasksFromFiles())
            {
                Debug.WriteLine(i);

                ScrapingTasksVMS.Add(new ScrapingTaskVM(i));
            }

            CSVResourcesVMS = new ObservableCollection<CSVResourceVM>(Workspace.Current.CSVResources.Select((sr) =>
           {
               return new CSVResourceVM(sr);
           }));

        }

        IPlugin MainPlugin;
        Workspace MainWorkspace;
        IEnumerable<ProductViewModel> ProductViewModels_arr = new List<ProductViewModel>();

        private async void onDirtyCSVResourceVMSelection()
        {
            //logic that need to be performed when some items changest's IsActive
            TotalRecordsCountString = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, int>(0, (v, i) => v + i.RowsCount).ToString();
            
            ProductViewModels_arr = CSVResourcesVMS.Where(i => i.IsActive).Aggregate<CSVResourceVM, IEnumerable<ProductViewModel>>(new List<ProductViewModel>(), (i, csvVM) => {
                return i.Concat(Utils.parseCSVfile(csvVM.FullPath).Select(p => new ProductViewModel(p)));
            });
            ProductViewModels = new ObservableCollection<ProductViewModel>(ProductViewModels_arr);
            notif(nameof(ProductViewModels));
            await Task.Delay(0);


        }
        private void handl_CSVResourceVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                onDirtyCSVResourceVMSelection();
            }
        }

        private void handl_CSVResourceVMS_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e )
        {
            foreach (var item in _CSVResourcesVMS)
            {
                item.PropertyChanged += (handl_CSVResourceVM_PropertyChanged);
            }
            onDirtyCSVResourceVMSelection();
        }



        public ObservableCollection<ProductViewModel> ProductViewModels { get {
                return new ObservableCollection<ProductViewModel>(
                    ProductViewModels_arr.Where(p=>p.Title.StartsWith(SearchQuery)));
            }
            set {
                notif(nameof(ProductViewModels));
            } } 

        private ObservableCollection<CSVResourceVM> _CSVResourcesVMS = new ObservableCollection<CSVResourceVM>();

        public ObservableCollection<CSVResourceVM> CSVResourcesVMS
        {
            get { return _CSVResourcesVMS;  }
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
            get { return Workspace.Current.Directory; }
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
                notif(nameof(ProductViewModels));
            }
            get { return _SearchQuery; }
        }


        private string _TargetPageQueryText;
        public string TargetPageQueryText
        {
            set { _TargetPageQueryText = value; notif(nameof(TargetPageQueryText)); }
            get { return _TargetPageQueryText; }
        }

       

      


        public ObservableCollection<ScrapingTaskVM> ScrapingTasksVMS { get; set; } = new ObservableCollection<ScrapingTaskVM>();


        public async void handleStartScrapingCommand()
        {
            var newScrapTask = MainPlugin.GetTask(TargetPageQueryText);
            var tvm = new ScrapingTaskVM(newScrapTask);
            ScrapingTasksVMS.Add(tvm);

            newScrapTask.Stage = ScrapTaskStage.DownloadingData;
            await newScrapTask.RunScraper();
            newScrapTask.Stage = ScrapTaskStage.ConvertingData;
            await newScrapTask.RunConverter();

           // Debug.WriteLine("return code is: scraper:" + res.ToString()+", converter:"+ res_c.ToString());
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
            if (!((website=="microcenter.com")|| (website == "www.microcenter.com"))) return false;
            return true;
        }
        private ICommand _StartScrapingCommand = null;
        /// <summary>
        /// starts scraing , takes no argument, only uses the query string
        /// </summary>
        public ICommand StartScrapingCommand { get {
                if(_StartScrapingCommand==null) _StartScrapingCommand = new MICommand(handleStartScrapingCommand, canExecuteStartScrapingCommand);

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
                    return new ProductViewModel(p);
                }).ToList();
                ProductViewModels_arr = new ObservableCollection<ProductViewModel> (lst);
                notif(nameof( ProductViewModels));
                

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
            Workspace.refresh();
            notif(nameof(CurrentWorkspaceDirectory));
            CSVResourcesVMS.Clear();
            foreach (var sr in Workspace.Current.CSVResources)
            {
                CSVResourcesVMS.Add(new CSVResourceVM(sr));
            }

        }
        public ICommand RefreshWorkspaceCommand { get { return new MICommand(handleRefreshWorkspaceCommand); } }



    }
}
