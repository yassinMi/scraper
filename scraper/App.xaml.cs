using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
    }
}
