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

        public static WebHelper instance { get; set; } = new WebHelper();
        public string GetPageTextSync(string pageUrl)
        {
            var t = this.GetStringAsync(pageUrl);

            t.ContinueWith(te => {
                AggregateException exception = t.Exception;
                Debug.WriteLine("ContinueWith faulted");
                throw exception.InnerException;
            }, TaskContinuationOptions.OnlyOnFaulted) ;

            return t.ContinueWith<string>( tt=> {
                Debug.WriteLine("ContinueWith true");
                return tt.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion).GetAwaiter().GetResult();

            
        }

        public Task<string> GetPageText(string pageUrl)
        {
            return this.GetStringAsync(pageUrl);
        }
    }
}
