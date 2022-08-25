using Mi.Common;
using scraper.Core.Utils;
using scraper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace scraper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Dictionary<string, string> CommandLineArgsDict { get; set; } = new Dictionary<string, string>();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CoreUtils.WriteLine($"scraper started [{DateTime.Now}]");

            Current.DispatcherUnhandledException += HandleException;
            AppDomain.CurrentDomain.AssemblyResolve += FindPluginAsm;
            int argc = e.Args.Count();
            if ((argc % 2) != 0)
            {
                throw new Exception($"wrong number of arguments: {argc }");
            }
            for(int i=0; i< argc; i+=2)
            {
                CommandLineArgsDict[e.Args[i]] = e.Args[i + 1];
            }
            PluginsManager.GlobalFolders = new string[] {
                ApplicationInfo.PLUGINS_GLOBAL_FOLDER_AT_MY_DOCUMENTS,
                ApplicationInfo.PLUGINS_GLOBAL_FOLDER_AT_INSTLLATION,
            };
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040)
            {
                e.Handled = true;
                CoreUtils.WriteLine($"UnhandledeExpection [ignored][{DateTime.Now}]: { e?.Exception}");
                return;
            }

            scraper.View.UnhandledErrorWindow w = new View.UnhandledErrorWindow();

            var hvm = new scraper.ViewModel.UnhandledErrorWindowVM();
            hvm.ExceptionObj = e.Exception;
            CoreUtils.WriteLine($"UnhandledeExpection[{DateTime.Now}]: { e?.Exception}");
            w.DataContext = hvm;
            w.ShowDialog();
            e.Handled = true;
            Debug.WriteLine(hvm.DialogChosedAction);
            App.Current.Shutdown(1);
            
        }

       

        private Assembly FindPluginAsm(object sender, ResolveEventArgs args)
        {
            string simpleName = new AssemblyName(args.Name).Name;
            var dll_abs_path = Path.Combine(ApplicationInfo.MAIN_PATH, "/plugins", simpleName + ".dll");
            if (!File.Exists(dll_abs_path)) return null;
            return Assembly.LoadFrom(dll_abs_path);
        }
    }
}
