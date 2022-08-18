using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Utils
{
    public interface IWebHelper
    {
        string GetPageTextSync(string pageUrl);

        Task<string> GetPageText(string pageUrl);
    }

    public class WebHelper:  HttpClient, IWebHelper
    {
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

        public static WebHelper instance { get; set; } = new WebHelper();
        public string GetPageTextSync(string pageUrl)
        {
            this.DefaultRequestHeaders.Add("user-agent", USER_AGENT);
            DefaultRequestHeaders.UserAgent.Clear();
          

            var t = this.GetStringAsync(pageUrl);
            Exception erorr = null;
            string result = null;
            var res =t.ContinueWith(te => {
                AggregateException exception = t.Exception;
                Debug.WriteLine($"ContinueWith faulted: {exception}, inner: {exception?.InnerException}");
                erorr= exception.InnerException;
            }, TaskContinuationOptions.OnlyOnFaulted) ;
            
            var f =t.ContinueWith( tt=> {
                Debug.WriteLine("ContinueWith true");
                result= tt.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            //t.Start();
            try
            {
                Task.WaitAll(res,f);
            }
            catch (Exception)
            {

                
            }
            
            if (erorr != null)
            {
                throw erorr;
            }
            else {
                if (result == null)
                {
                    throw new Exception("unknown result and error");
                }
                else
                {
                    return result;
                }
            }
           /*
            catch (AggregateException er)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("the folowing errors happened:");
                foreach (var item in er.Flatten().InnerExceptions)
                {
                    sb.AppendLine($"{ item.GetType()}:{item.Message}");
                }
                Debug.WriteLine(sb.ToString());
                if(er.Flatten().InnerExceptions.Any(e=>e is HttpRequestException){
                    throw new Exception(sb.ToString());
                }
                else { throw new Exception("unknow error happened"); };
                
            }*/
            
        }

        public Task<string> GetPageText(string pageUrl)
        {
            return this.GetStringAsync(pageUrl);
        }
    }
}
