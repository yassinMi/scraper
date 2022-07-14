using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Model;
using scraper.Plugin;
using scraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.ViewModel.Tests
{
    [TestClass()]
    public class MainViewModelTests
    {
        [TestMethod()]
        public void ScrapingNew()
        {
            var ws = Workspace.GetWorkspace(@"E:\TOOLS\scraper\tests.yass\myTestWorkspace");
            var p = new FakePlugin();
            var mvm = new MainViewModel(p, ws);
            string targetPageUrl = TestUrls.GetRandomUrl();
            mvm.TargetPageQueryText = targetPageUrl;
            mvm.StartScrapingCommand.Execute(null);
            Task.Delay(50 * 300 + 1300 + 2000).GetAwaiter().GetResult();
            var o = mvm.ScrapingTasksVMS.FirstOrDefault(t => t.Model.TargetPage == targetPageUrl);
            Assert.IsTrue(mvm.ScrapingTasksVMS.Any(),"the task vm collection is empty");
            Debug.WriteLine($"the first tvm has TargetPage: {mvm.ScrapingTasksVMS.First().Model?.TargetPage}");
            Assert.IsTrue(o!=null,"no task vm with the specified target page is found");
            Assert.IsTrue(o.DownloadProgress.Current == 49,"the final download prog is not correct");
            
        }
        [TestMethod()]
        public void AvailableData()
        {
            MainViewModel mvm = new MainViewModel();
            Assert.IsTrue(mvm.CSVResourcesVMS.Count > 10);
            foreach (var item in mvm.CSVResourcesVMS)
            {
                item.IsActive = true;
            }
            Debug.WriteLine($"number of items: {mvm.ProductViewModels.Count}");
            Assert.IsTrue(mvm.ProductViewModels.Count > 10);
        }
    }
}