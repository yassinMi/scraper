using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using scraper.Core.Workspace;

namespace scraper.Core.Utils
{
    public class PluginsManager
    {

        private static Core.Plugin[] _CachedGlobalPlugins;
        /// <summary>
        /// using this is safer than GetGlobalPlugins, by means of preventing insatiating duplicate plugin instance by pre-loading all plugins into a static array
        /// </summary>
        public static Core.Plugin[] CachedGlobalPlugins { get
            {
                if (_CachedGlobalPlugins ==null) _CachedGlobalPlugins = GetGlobalPlugins().ToArray();
                return _CachedGlobalPlugins;
            } }


        public static string[] GlobalFolders { get; set; } = null;
        /// <summary>
        /// returns set of IPlugin instances that correspond to the dll files under MyDocmuments objects to be used by the rest of the app
        /// </summary>
        /// <returns></returns>
        /// 
        public static IEnumerable<Core.Plugin> GetGlobalPlugins()
        {
            if (GlobalFolders == null) throw new Exception("PluginsManager: attempting to GetGlobalPlugins() with GlobalFolders static property not set");
            
            //
            foreach (var GlobalFoldder in GlobalFolders)
            {
                Debug.WriteLine($"checking global folder '{GlobalFoldder}'");
                if (!Directory.Exists(GlobalFoldder))
                {
                    Directory.CreateDirectory(GlobalFoldder);
                    Debug.WriteLine($"Created global folder '{GlobalFoldder}'");
                }
                foreach (var f in Directory.GetFiles(GlobalFoldder))
                {
                    if (Path.GetExtension(f).ToLower().Replace(".", "") == "dll")
                    {
                        object pluginInstance = null;
                        try
                        {
                            var asm = Assembly.LoadFrom(f);
                            var first_IPluginType = asm.GetTypes().FirstOrDefault(t => typeof(Core.Plugin).IsAssignableFrom(t));
                            if (first_IPluginType == null) continue;
                            pluginInstance = Activator.CreateInstance(first_IPluginType);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("error loding assembly: " + f + ": " + e.Message);
                        }
                        if (pluginInstance != null)
                            yield return (Core.Plugin)pluginInstance;
                    }

                }

            }
            

            
        }

        public  static Core.Plugin TryLoadFromFile(string f)
        {
            if (!(Path.GetExtension(f).ToLower().Replace(".", "") == "dll"))
            {
                return null;
            }
                object pluginInstance = null;
            try
            {
                var asm = Assembly.LoadFrom(f);
                var first_IPluginType = asm.GetTypes().FirstOrDefault(t => typeof(Core.Plugin).IsAssignableFrom(t));
                if (first_IPluginType == null) return null;
                pluginInstance = Activator.CreateInstance(first_IPluginType);
            }
            catch (Exception e)
            {

                Debug.WriteLine("error loding assembly: " + f + ": " + e.Message);
                return null;
            }
            if (pluginInstance != null) return pluginInstance as Core.Plugin;
            return null;
        }

       
    }
}
