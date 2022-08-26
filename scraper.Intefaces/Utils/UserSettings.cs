using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Utils
{

    public enum CachePolicy { All, None, ElementsPagesOnly }
    public enum WebClientProtocol { Default, tls12, tls12http1 }
    public class UserSettings
    {
        static UserSettings current;
        public static UserSettings Current
        {
            get
            {
                if (current == null)
                {
                    current = loadOrFactory();
                }
                return current;
            }

        }

        

        public CachePolicy CachePolicy {get;set;}
        /// <summary>
        /// temporary workaround
        /// </summary>
        public WebClientProtocol WebClientProtocol { get; set; }
        private static UserSettings getFactoryUserSettings()
        {
            UserSettings s = new UserSettings();
            s.CachePolicy = CachePolicy.All;
            s.WebClientProtocol = WebClientProtocol.tls12;
            return s;
        }

        static object lock_ = new object();
        const string SETTINGS_FILE =  "./user.settings.json";
        /// <summary>
        /// load file or make new file with factory settings, 
        /// </summary>
        /// <returns></returns>
        private static UserSettings loadOrFactory()
        {
            //#if file exists and valid
            lock (lock_)
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(SETTINGS_FILE));
                }
                catch (Exception)
                {
                    var s = getFactoryUserSettings();
                    string s_str = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                    File.WriteAllText(SETTINGS_FILE, s_str);
                    return s;
                }
            }
            

            //else
        }
        /// <summary>
        /// saves current into file
        /// </summary>
        public void save()
        {
            lock (lock_)
            {
                string s_str = Newtonsoft.Json.JsonConvert.SerializeObject(Current);
                File.WriteAllText(SETTINGS_FILE, s_str);
            }
        }
    }
}
