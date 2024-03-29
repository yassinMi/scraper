﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Trinet.Core.IO.Ntfs;

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


        //i needed this backing delegate to alter the default event behaviour preventing accidentaly adding more than one subscriber
        //it's always only one subscriber: for tests a dummy one,
        //at runtime a real subscriper is added and is performing the UI interactions.
        private static EventHandler<UI.PromptRequestEventArgs> promptRequested;
        
        public static event EventHandler<UI.PromptRequestEventArgs> PromptRequested {
            add { promptRequested = value; }
            remove { promptRequested = null; }
        }
        
        public static void RequestPrompt(UI.PromptContent promptContent, Action<string> responseHandler)
        {
            if (promptRequested == null)
            {
                responseHandler("default");
                return;
            }
            promptRequested.Invoke(null, new UI.PromptRequestEventArgs(promptContent, responseHandler));
        }
        

        const string AuxiliaryTask_Query_Separator = "`,";
        public static bool TryParseAuxiliaryTaskQuery(string q, out string header, out string[] parameters)
        {
            var all = Regex.Split(q,AuxiliaryTask_Query_Separator);
            if(all.Count()<2)
            {
                header = null; parameters = null;
                return false;
            }
            header = all.First();
            parameters = all.Skip(1).ToArray();
            return true;
        }
        public static string FormatAuxiliaryTaskQuery(string header, params string[] parameters)
        {
            var auxiliaryTaskQueryBuilder = new StringBuilder();
            auxiliaryTaskQueryBuilder.Append(header);//auxiliary task type identifier
            auxiliaryTaskQueryBuilder.Append(AuxiliaryTask_Query_Separator);
            auxiliaryTaskQueryBuilder.Append(string.Join(AuxiliaryTask_Query_Separator,parameters));
            return auxiliaryTaskQueryBuilder.ToString();
        }

        static string logFile = "log.txt";
        public static void WriteLine(string line)
        {
            Trace.WriteLine(line);
            File.AppendAllLines(logFile, new string[] { line});
        }
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

        /// <summary>
        /// removes the data streams that flags the file as downloaded form the internet
        /// the data is
        /// [ZoneTransfer]
        /// ZoneId=3
        /// the stream name is Zone.Identfier
        //// can be read using command more < myFile.dll:Zone.Identfier
        //// can be written using (echo [ZoneTransfer] && echo ZoneId=3) > myFile.dll:Zone.Identfier 
        /// </summary>
        /// <param name="file"></param>
        public static void UnblockFile(FileInfo file)
        {
            file.DeleteAlternateDataStream("Zone.Identifier");
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
