using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using scraper.Core.Workspace;
using scraper.Core.Attributes;

namespace scraper.Core.Utils
{
    public class PluginsManager
    {
        /// <summary>
        /// used by the function that determines plugin compatibility based on the verison decaled as assemply attribute
        /// </summary>
        public static bool IsCompatibleSemVer(string apiVersion, string pluginTargetApiVersion)
        {
            Version api = new Version(apiVersion);
            Version plugin = new Version(pluginTargetApiVersion);
            Debug.WriteLine($"comparing versions: api:{api.Major}.{api.Minor}.{api.Build} , plugin:{plugin.Major}.{plugin.Minor}.{plugin.Build}");
            //# plugin is more recent than core: not compatible
            if (api < plugin) return false;

            //# core is more recent than plugin : ~semVer rules 

            //before 0.2.x exclusvly breaking changes are implied with patch level increase
            //before major version 1 exclusvly and after v0.2.x inclusively breaking changes are impllied with minor increase,
            //after major 1 inclusivly breaking changes are impllied with major increase
            if (api.Major==0 && plugin.Major == 0)
            {
                if (api.Minor < 2 && plugin.Minor < 2)
                {
                    //case1
                    return api.Build == plugin.Build;//'build' is semver patch
                }
                else
                {
                    //case2
                    return api.Minor == plugin.Minor;
                }
            }
            //case3
            return (api.Major == plugin.Major) ;

        }
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
                            var cav_attr = asm.GetCustomAttribute<CoreAPIVersionAttribute>();
                            if (cav_attr == null) { Debug.WriteLine("assembly : " + f + " ignored because it's missing the CoreAPIVersionAttribute "); continue; };//not a plugin assembly, ignored
                            bool isCompatible = IsCompatibleSemVer(CoreUtils.GetCurrentCoreAPIVersion(), cav_attr.APIVersion);
                            if (isCompatible == false) throw new Exception($"plugin assembly {f} targets a different API version '{cav_attr.APIVersion}', current core api is: '{CoreUtils.GetCurrentCoreAPIVersion()}'");
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
                CoreUtils.UnblockFile(new FileInfo(f));
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
