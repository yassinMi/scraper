﻿using BusinesslistPhPlugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Core;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [TestCategory("results")]
        [TestMethod]
        public void CorrectResults()
        {
            var h = CoreUtils.getUniqueLinkHash(@"https://www.businesslist.ph/company/296663/basilio-rentals-and-sales-corp");
            Debug.WriteLine(h);
            PluginTestsUtils.initialize();
            var tws = PluginTestsUtils.loadOrCreateTestsWorkspace(BLScraperTestsData.bl_tests_ws_path, BLScraperTestsData.pluginName);
            foreach (var known_page in BLScraperTestsData.KnownPages())
            {
                var ttask = tws.Plugin.GetTask(known_page.targetPageUrl);
                var sw = Stopwatch.StartNew();
                ttask.RunScraper(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                Debug.WriteLine($"RunScraper for known page:'{known_page.targetPageUrl}' took {sw.Elapsed}");
                Assert.IsTrue(File.Exists(ttask.ActualOutputFile),$"task didn't generate the file expected at '{ttask.ActualOutputFile}'");
                Assert.IsTrue(File.ReadAllText(ttask.ActualOutputFile) == File.ReadAllText(known_page.csvRef), $"generated file content at '{ttask.ActualOutputFile}' diffres from the csv.ref counterpart at '{known_page.csvRef}'");
                                

            }
        }
    }
   
}
