using BusinesslistPhPlugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Core;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
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
        internal string csvRef;
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
                int result_count = BLScrapingTask.EnumeratePagesSTBF(known_page.targetPageUrl).Count();
                bool correct = result_count == known_page.pagesCount;
                Assert.IsTrue(correct, $"test failed on targetpage: {known_page.targetPageUrl}, expected {known_page.pagesCount}, got {result_count}");
            }
        }

        [TestMethod]
        public void CorrectResults()
        {
            PluginTestsUtils.initialize();
            var tws = PluginTestsUtils.loadOrCreateTestsWorkspace(BLScraperTestsData.bl_tests_ws_path, BLScraperTestsData.pluginName);
            foreach (var known_page in BLScraperTestsData.KnownPages())
            {
                var ttask = tws.Plugin.GetTask(known_page.targetPageUrl);
                
                 ttask.RunScraper(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                Assert.IsTrue(File.Exists(ttask.ActualOutputFile),$"task didn't generate the file expected at '{ttask.ActualOutputFile}'");
                Assert.IsTrue(File.ReadAllText(ttask.ActualOutputFile) == File.ReadAllText(known_page.csvRef), $"generated file content at '{ttask.ActualOutputFile}' diffres from the csv.ref counterpart at '{known_page.csvRef}'");
                                

            }
        }
    }
   
}
