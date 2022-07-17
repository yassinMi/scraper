using Mi.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

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
            AppDomain.CurrentDomain.UnhandledException += HandleException;
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
        }

        private void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            scraper.View.UnhandledErrorWindow w = new View.UnhandledErrorWindow();
            w.ShowDialog();
            
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
