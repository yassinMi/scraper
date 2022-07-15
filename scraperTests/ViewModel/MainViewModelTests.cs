using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Model;
using scraper.Plugin;
using scraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        [TestMethod()]
        public void CSVResourcesVMS_gets_populated()
        {
            const int number_of_csv_files_in_ws = 22;
            Debug.WriteLine(typeof(string).Assembly.Location);
            Debug.WriteLine(typeof(Debug).Assembly.Location);
            Debug.WriteLine(typeof(System.Reflection.Assembly).Assembly.Location);

            Debug.WriteLine(typeof(App).Assembly.Location);
            Debug.WriteLine(typeof(FakePlugin).Assembly.Location);

            MainViewModel mvm = new MainViewModel();
            //mvm.RefreshWorkspaceCommand.Execute(null);
            Debug.WriteLine("count:" + mvm.CSVResourcesVMS.Count.ToString());
            Assert.IsTrue(mvm.CSVResourcesVMS.Count == 22);
            Assert.IsTrue(mvm.CurrentWorkspaceDirectory.ToLower() ==  @"E:\TOOLS\scraper\scraper\scripts".ToLower(),"wrong workspace dir : "+ mvm.CurrentWorkspaceDirectory);

            //test case 2
            var ws = Workspace.GetWorkspace(@"E:\TOOLS\scraper\tests.yass\myTestWorkspace");
            var p = new FakePlugin();
            MainViewModel mvm2 = new MainViewModel(p,ws);
            //mvm.RefreshWorkspaceCommand.Execute(null);
            Assert.IsTrue(mvm2.CSVResourcesVMS.Count == 0);
            Assert.IsTrue(mvm2.CurrentWorkspaceDirectory.ToLower() == @"E:\TOOLS\scraper\tests.yass\myTestWorkspace".ToLower(), "wrong workspace dir2" + mvm2.CurrentWorkspaceDirectory);


            //test case 3
            var ws3 = Workspace.GetWorkspace(@"E:\TOOLS\scraper\scraper\scripts");
            MainViewModel mvm3 = new MainViewModel(p, ws3);
            //mvm3.RefreshWorkspaceCommand.Execute(null);
            Assert.IsTrue(mvm3.CSVResourcesVMS.Count == 22, "mvm3 wrong CSVResourcesVMS count");


        }
        [TestMethod()]
        public void GlobalUserPluginsGetsLoadedFromMydocuments()
        {
            MainViewModel mvm = new MainViewModel();
            Assert.IsTrue(mvm.GlobalUserPlugins.Count == 1,"wrong plugins count");
            var firstPluginName = mvm.GlobalUserPlugins.First().Name;
            Assert.IsTrue(firstPluginName == "FakePluginExternal",$"unexcpected plugin name {firstPluginName}");
        }
    }
}