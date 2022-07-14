using scraper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model.Common;

namespace scraperTests.Plugin
{
    public class FakePlugin : IPlugin
    {
        public event EventHandler<string> OnError;
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnTaskDetail;

        async public Task RunConverter(string pageUrl)
        {
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(300);
                OnProgress?.Invoke(this, new DownloadingProg() { Total = 50, Current = i });
                OnTaskDetail?.Invoke(this, $"fake download: file{i + 1}.zip");
            }
            
        }

        async public Task RunScraper(string pageUrl)
        {
            throw new NotImplementedException();
        }
    }
}
