using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{
    public static class PluginTestsUtils
    {

        /// <summary>
        /// must be called before any plugin testing code.
        /// assigns the PluginsManager.GlobalFolders like scraper UI project does at application startup
        /// </summary>
        public static void initialize()
        {
            string plugins_folder_in_debug = @"E:\TOOLS\scraper\scraper\bin\Debug\Plugins";
            string plugin_folder_in_mydocuments = @"C:\Users\CAT\Documents\MiScraper\Plugins";
            PluginsManager.GlobalFolders = new string[] { plugins_folder_in_debug, plugin_folder_in_mydocuments };

        }





        /// <summary>
        /// loads/creates and makes current ws using the specified ws_path path and (in creating case) pluginName
        /// </summary>
        public static Workspace loadOrCreateTestsWorkspace(string tests_ws_path,string pluginName)
        {
            if (Workspace.Exists(tests_ws_path))
            {
                Workspace.MakeCurrent(Workspace.Load(tests_ws_path));
                return Workspace.Current;
            }
            else
            {
                var plugin = PluginsManager.GetGlobalPlugins().FirstOrDefault(p => p.Name == pluginName);
                Workspace.MakeCurrent(Workspace.CreateOne(tests_ws_path, plugin));
                return Workspace.Current;
            }

        }

    }
}
