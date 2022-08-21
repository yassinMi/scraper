//v1
//started versioning 25-jun-2022 scraper project
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mi.Common
{


    /// <summary>
    /// Mi reusable class contains the info in the about view and other stuff
    /// uncomment stuff that's relevent to the project
    /// </summary>
    public class ApplicationInfo
    {

        public ApplicationInfo()
        {
            // Environment.CurrentDirectory
        }

        public static bool IsDev { get; set; } = false;
        public static string APP_DEV_NAME = "Scraper"; //used in creating app data directory and such

        //these fields are to be displayed to the user 
        public static string APP_TITLE { get;  } = "Scraper";
        public static string APP_SUB_TITLE { get;  } = "© Mi 2022 ";
        public static string APP_SHORT_DESCRIPTION { get; } = "An extensible, plugin-driven GUI web scraping tool";
        public static string APP_VERSION_NOTE { get; } = "";
    
        public static string APP_VERSION { get; } = "0.1.3 " + (IsDev ? " [dev]" : "(21-08-2022)");
        public static string APP_DEVELOPER_NAME { get; set; } = "YassinMi";
        public static string APP_GUI_DESIGNER_NAME { get; set; } = "YassinMi";
        public static string APP_GITHUB_URL { get; set; } = "https://github.com/yassinMi/scraper";
        public static string APP_DEVELOPER_EMAIL { get; set; } = "DIR16CAT17@gmail.com";
        //public static int Host_Rendering_Tier { get; set; } = RenderCapability.Tier >> 16;
    
        /// <summary>
        /// the absolute path where the exe lives
        /// </summary>
        public static readonly string MAIN_PATH = Path.GetDirectoryName(
           System.Reflection.Assembly.GetExecutingAssembly().Location);

        //public static string CURL_PATH = MAIN_PATH + "\\curl\\curl.exe";
        public static readonly string APP_DATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mi\\"+APP_DEV_NAME;
        //public static string DEFAULT_GLOBAL_OUTPUT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CLW Output";
        //public static string CLW_PRESETS_DIR = MAIN_PATH + "\\CLW Presets";
        public static readonly string APP_CONFIG_FILE = APP_DATA + "\\"+APP_DEV_NAME+".config.mi.xml";
        //internal static string TEMP_HTML_FILES = APP_DATA + "\\Temp HTML";
        //internal static string SCRIPTS_DIR = MAIN_PATH + "\\scripts";
        //internal static object SFX_DIRECTORY = MAIN_PATH + "\\SFX";
        internal static readonly string ERRORS_LOG_FILE = APP_DATA + @"\Errors.log";
        /// <summary>
        /// under MyDocuments/MiScraper/Plugins
        /// </summary>
        public static string PLUGINS_GLOBAL_FOLDER_AT_MY_DOCUMENTS = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+ @"\MiScraper\Plugins");
        public static string PLUGINS_GLOBAL_FOLDER_AT_INSTLLATION = MAIN_PATH + @"\Plugins";

        internal static void OnAppStartup()
        {
            if (Directory.Exists(APP_DATA) == false)
            {
                Directory.CreateDirectory(APP_DATA);
            }
        }
    }





}
