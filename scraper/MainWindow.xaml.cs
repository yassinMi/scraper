using Mi.Common;
using scraper.Core;
using scraper.Core.Workspace;
using scraper.Model;
using scraper.Plugin;
using scraper.Services;
using scraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace scraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Debug.WriteLine("starting main window");
            InitializeComponent();

            string ws_dir = getStartupWs();
            Debug.WriteLine("getting startup ws dir retured: " + ws_dir);
            if (ws_dir == null)
            {
                Debug.WriteLine("no starting ws dir could be determined");
                Debug.WriteLine("instantiating a fresh MainVewModel (setup screen mode)");
                DataContext = new MainViewModel();
            }
            else
            {
                Debug.WriteLine($"starting ws dir is{ws_dir}");
                Debug.WriteLine($"loading workspace at {ws_dir}");
                var ws = Workspace.Load(ws_dir);
                Workspace.MakeCurrent(ws);
                Core.Plugin plugin = ws.Plugin = PluginsManager.CachedGlobalPlugins.FirstOrDefault(p => p.Name == ws.PluginsNames.FirstOrDefault());
                Trace.Assert(plugin != null, "failed to load any plugins into the workspace, make sure to have a .scraper/plugins file pointing to existing global plugins");
                 //new BLScraper() { WorkspaceDirectory = ws.Directory };
                DataContext = new MainViewModel(plugin,ws);
            }

            
            
            
     

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight-8;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            ((MainViewModel)DataContext).mw = this;
            Debug.WriteLine("endof mw ctor");
        }

        /// <summary>
        /// the logic used to select what workspace the app will start with, 
        /// first cheks cmd, then config, the null which indicates that theui should show the ws setup mode
        /// </summary>
        /// <returns></returns>
        string getStartupWs()
        {
            
            string blWSpath = @"E:\TOOLS\scraper\tests.yass\blWorkspace";
            string generalWStestPath = @"E:\TOOLS\scraper\tests.yass\myTestWorkspace";
            // App.Current.

            //commandline, config, null
            string cmdws_path;
            if ((App.Current as App).CommandLineArgsDict.TryGetValue("-workspace", out cmdws_path) == true)
            {
                return cmdws_path;
            }
            if (string.IsNullOrWhiteSpace(ConfigService.Instance.WorkspaceDirectory)==false)
            {
                return ConfigService.Instance.WorkspaceDirectory;
            }
            return null;
        }

        Point _startPosition;
        bool _isResizing = false;

       

        private void resizeGrip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Mouse.Capture(resizeGrip))
            {
                _isResizing = true;
                _startPosition = Mouse.GetPosition(this);
            }
        }

        private void window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                Point currentPosition = Mouse.GetPosition(this);
                double diffX = currentPosition.X - _startPosition.X;
                double diffY = currentPosition.Y - _startPosition.Y;
                //double currentLeft = gridResize.Margin.Left;
                //double currentTop = gridResize.Margin.Top;
                //gridResize.Margin = new Thickness(currentLeft + diffX, currentTop + diffY, 0, 0);
                _startPosition = currentPosition;
            }
        }

        private void resizeGrip_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing == true)
            {
                _isResizing = false;
                Mouse.Capture(null);
            }

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }

        private void Grid_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
            maximizeButtImg.Source = new BitmapImage(
                new Uri($"pack://application:,,,/media/img/{(WindowState == WindowState.Maximized ? "normal" : "maximize")}-10-white.png"));

        }

        private void mnimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
             var process = new Process();

            var processInfo = new ProcessStartInfo("ping.exe", "localHost");
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (s, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (s, args) => Console.WriteLine(args.Data);
            process.Exited += (s, args) => Console.WriteLine("ping finished.");

            process.Start();
            process.BeginOutputReadLine();
            

        }

        private void dragging_area_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }

        private void ResultsControlSection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void dragging_area_right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void DataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        
        {
            var currentCell = e.ClipboardRowContent[elementsDatagrid.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);
        }

        private void elementsDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dc = this.DataContext as MainViewModel;
            if (dc != null)
            {
                dc.SelectionCount = elementsDatagrid.SelectedItems.Count;
                dc.DataGridSelectedItemsRef = elementsDatagrid.SelectedItems.Cast<ElementViewModel>();
            }
            
        }

        private void WorkspaceSetupPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string text = (sender as TextBlock)?.Text;
            if (text != null)
                Clipboard.SetText(text);
        }
    }
}
