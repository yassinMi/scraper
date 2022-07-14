using scraper.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    /// <summary>
    /// supported pattern is: download a page's static elements
    /// </summary>
    public interface IPlugin
    {
        event EventHandler<DownloadingProg> OnProgress;
        event EventHandler<string> OnTaskDetail; //stdout prints something like T: downloading image.jpg
        event EventHandler<string> OnError;  //stderr prints something
        /// <summary>
        /// downloads the raw objects containing the required flieds, saves them under workspace/raw or workspace/{ElementName}-raw
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        Task RunScraper(string pageUrl);
        Task RunConverter(string pageUrl);

    }
}
