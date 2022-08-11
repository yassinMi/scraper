using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace scraper.Core.Utils
{

    
   

    //@configurator way
    public class Synchronizer<T>
    {
        private Dictionary<T, object> locks;
        private object myLock;

        public Synchronizer()
        {
            locks = new Dictionary<T, object>();
            myLock = new object();
        }

        public object this[T index]
        {
            get
            {
                lock (myLock)
                {
                    object result;
                    if (locks.TryGetValue(index, out result))
                        return result;

                    result = new object();
                    locks[index] = result;
                    return result;
                }
            }
        }
    }
    public static class CoreUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>null in case the attribute is missing </returns>
        public static string GetCurrentCoreAPIVersion()
        {
            var cav = typeof(Plugin).Assembly.GetCustomAttribute<Attributes.CoreAPIVersionAttribute>();
            return cav?.APIVersion;
        }
        public static void se()
        {
            var opts = new ChromeOptions() { };
            //opts.AddArguments("headless","disable-gpu","no-sandbox");
            
            WebDriver wd = new ChromeDriver(opts);
            wd.Url = @"https://www.google.com/search?q=best+software+testing+tools&rlz=1C1CHWL_enMA1011MA1011&oq=best+software+testing+tools&aqs=chrome..69i57.56629j0j7&sourceid=chrome&ie=UTF-8";
            Debug.WriteLine("ok");
            wd.Navigate();
            Debug.WriteLine("navigated");

            wd.GetScreenshot().SaveAsFile("se-screenshot.png", ScreenshotImageFormat.Png);
            StringBuilder sb = new StringBuilder();
            
            Debug.WriteLine(sb.ToString());
            Debug.WriteLine("saved");
            var w = new OpenQA.Selenium.Support.UI.WebDriverWait(wd, TimeSpan.FromSeconds(50));
              
            //Thread.Sleep(5000);
            //wd.Close();
            //wd.Dispose();
            
            
            

        }

        public static NameValueCollection parseQueryString(string q)
        {
            return HttpUtility.ParseQueryString(q);
        }


        private static readonly Regex InvalidFileRegex = new Regex(
    string.Format("[{0}]", Regex.Escape(@"<>:""/\|?*")));


        public static string SanitizeFileName(string fileName)
        {
            return InvalidFileRegex.Replace(fileName, string.Empty);
        }

        //from @Philip Rieck resp
        public static string CamelCaseToUIText(string camelCase)
        {
            string res=  Regex.Replace(camelCase, @"(?<a>(?<!^)((?:[A-Z][a-z])|(?:(?<!^[A-Z]+)[A-Z0-9]+(?:(?=[A-Z][a-z])|$))|(?:[0-9]+)))", @" ${a}");
            res = res.Substring(0,1).ToUpper()+ res.Substring(1);
            return res;
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);


                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }



        /// <summary>
        /// only required by downloadOrReadFromObject, returns a filesystem-friendly hash
        /// </summary>
        public static string getUniqueLinkHash(string businessLink)
        {
            return CreateMD5(businessLink);
        }


        public static string StripHTML(string htmlStr)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlStr);
            var root = doc.DocumentNode;
            string s = "";
            foreach (var node in root.DescendantsAndSelf())
            {
                if (!node.HasChildNodes)
                {
                    string text = node.InnerText;
                    if (!string.IsNullOrEmpty(text))
                        s += text.Trim() + " ";
                }
            }
            return s.Trim();
        }


    }
}
