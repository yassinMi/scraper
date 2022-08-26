using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Utils
{

    
    public enum CachePolicy { All, None, ElementsPagesOnly }
    public enum WebClientProtocol { Default, Tls12, Tls12http1,
        Tls
    }
    /// <summary>
    /// auto updating file when dirty
    /// </summary>
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

        private void OnDirty()
        {
            save();
        }

        private CachePolicy _CachePolicy;
        public CachePolicy CachePolicy
        {
            get { return _CachePolicy; }
            set
            {
                if (_CachePolicy != value)
                {
                    _CachePolicy = value;
                    if (lock_save == false)
                        OnDirty();
                }

            }
        }
        private WebClientProtocol _WebClientProtocol;
        /// <summary>
        /// setting true will avoid saving.. workaround a stackoverflowexcepion
        /// </summary>
        static bool lock_save = false;
        /// <summary>
        /// temporary workaround
        /// </summary>
        public WebClientProtocol WebClientProtocol {
            get { return _WebClientProtocol; }
            set
            {
                if (_WebClientProtocol != value)
                {
                    _WebClientProtocol = value;
                    if(lock_save==false)
                    OnDirty();
                }

            }
        }
        private static UserSettings getFactoryUserSettings()
        {
            
            UserSettings s = new UserSettings();
            s._CachePolicy = CachePolicy.All;//it is immoprtant to acess backing fiels to avoid stackoverflowexception
            s._WebClientProtocol = WebClientProtocol.Tls12;
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
                lock_save = true;
                try
                {
                    var r = Newtonsoft.Json.JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(SETTINGS_FILE));
                    lock_save = false;
                    return r;
                }
                catch (Exception)
                {
                    var s = getFactoryUserSettings();
                    string s_str = Newtonsoft.Json.JsonConvert.SerializeObject(s);
                    File.WriteAllText(SETTINGS_FILE, s_str);
                    lock_save = false;
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
