using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private static readonly Regex InvalidFileRegex = new Regex(
    string.Format("[{0}]", Regex.Escape(@"<>:""/\|?*")));


        public static string SanitizeFileName(string fileName)
        {
            return InvalidFileRegex.Replace(fileName, string.Empty);
        }

        //from @Philip Rieck resp
        public static string CamelCaseToUIText(string camelCase)
        {
            return Regex.Replace(camelCase, @"(?<a>(?<!^)((?:[A-Z][a-z])|(?:(?<!^[A-Z]+)[A-Z0-9]+(?:(?=[A-Z][a-z])|$))|(?:[0-9]+)))", @" ${a}");
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
