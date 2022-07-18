using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Services
{
    public interface IWebHelper
    {
        string GetPageTextSync(string pageUrl);

        Task<string> GetPageText(string pageUrl);
    }

    public class WebHelper:  HttpClient, IWebHelper
    {

        public string GetPageTextSync(string pageUrl)
        {
            return this.GetStringAsync(pageUrl).GetAwaiter().GetResult();
        }

        public Task<string> GetPageText(string pageUrl)
        {
            return this.GetStringAsync(pageUrl);
        }
    }
}
