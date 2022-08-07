using BusinesslistPhPlugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Core;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{

    public class TargetPageBLScraperTestData
    {
        public string targetPageUrl;
        public int pagesCount;
        public int elementsCount;
    }

    [TestClass]
    public class BLScraperTests
    {
        [TestMethod]
        public void EnumeratePagesCorrectCount()
        {
            PluginTestsUtils.initialize();
            PluginTestsUtils.loadOrCreateTestsWorkspace(BLScraperTestsData.bl_tests_ws_path,BLScraperTestsData.pluginName);
            foreach (var known_page in BLScraperTestsData.KnownPages())
            {
                int result_count = BLScrapingTask.EnumeratePages(known_page.targetPageUrl).Count();
                bool correct = result_count == known_page.pagesCount;
                Assert.IsTrue(correct, $"test failed on targetpage: {known_page.targetPageUrl}, expected {known_page.pagesCount}, got {result_count}");
            }
          
            
            
        }
    }
   
}
