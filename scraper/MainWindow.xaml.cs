using scraper.Model;
using scraper.Plugin;
using scraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            var ws = Workspace.GetWorkspace(@"E:\TOOLS\scraper\tests.yass\myTestWorkspace");
            var p = new FakePlugin();
            InitializeComponent();
            
            DataContext = new MainViewModel(p,ws);
            ((MainViewModel)DataContext).mw = this;
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
    }
}
