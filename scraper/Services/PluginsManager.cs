using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Core;
using Mi.Common;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace scraper.Services
{
    public class PluginsManager
    {
        /// <summary>
        /// returns set of IPlugin instances that correspond to the dll files under MyDocmuments objects to be used by the rest of the app
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPlugin> GetGlobalPlugins()
        {
            var GlobalFoldder = ApplicationInfo.PLUGINS_GLOBAL_FOLDER;
            //
            Debug.WriteLine($"docs folder '{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}'");
            Debug.WriteLine($"checking global folder '{GlobalFoldder}'");
            if (!Directory.Exists(GlobalFoldder)){
                Directory.CreateDirectory(GlobalFoldder);
                Debug.WriteLine($"Created global folder '{GlobalFoldder}'");
                yield break;
            }

            foreach(var f in Directory.GetFiles(GlobalFoldder))
            {
                if (Path.GetExtension(f).ToLower().Replace(".", "") == "dll")
                {
                    var asm = Assembly.LoadFrom(f);
                    var first_IPluginType = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t));
                    if (first_IPluginType == null) continue;
                    object pluginInstance = Activator.CreateInstance(first_IPluginType);
                    yield return (IPlugin)pluginInstance;
                }
                    
            }
        }

    }
}
