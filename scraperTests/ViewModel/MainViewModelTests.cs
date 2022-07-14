using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Model;
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
            var p = 
            var mvm = new MainViewModel( ws);
            mvm.SearchQuery = TestUrls.GetRandomUrl();
            mvm.StartScrapingCommand.Execute(null);
            
            
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