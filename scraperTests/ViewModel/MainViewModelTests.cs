using CsvHelper;
using HtmlAgilityPack;
using Mi.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Model;
using scraper.Plugin;
using scraper.Services;
using scraper.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scraper.ViewModel.Tests
{


    public class Fruit
    {

    }

    public class Apple : Fruit
    {

    }

    public class Orange : Fruit
    {

    }
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
            Debug.WriteLine($"number of items: {mvm.BusinessesViewModels.Count()}");
            Assert.IsTrue(mvm.BusinessesViewModels.Count() > 10);
        }

        [TestMethod()]
        public void ListAllInfoKeys()
        {

            List<string> keys = new List<string>();
            foreach (var item in union())
            {
                foreach (var i in BLScrapingTask.getInfos(item.Value))
                {
                    if (!keys.Contains(i.Item1)) keys.Add(i.Item1);
                } 
            }
            Debug.WriteLine($"{keys.Count} keys found in the union elements:");
            foreach (var k in keys)
            {
                Debug.WriteLine(k);
            }
        }

        [TestMethod()]
        public void testInfosEnumerator()
        {
            /* Workspace.MakeCurrent(@"E:\TOOLS\scraper\tests.yass\blWorkspace");
             var raw = BLScrapingTask.downloadOrRead(@"https://www.businesslist.ph/company/301559/automation-and-security-inc",
             Workspace.Current.GetHtmlObjectsFolder());
             HtmlDocument doc = new HtmlDocument();
             doc.LoadHtml(raw);
             foreach (var item in BLScrapingTask.getInfos(doc.DocumentNode))
             {
                 Debug.WriteLine(item.Item1);
                 if (item.Item1 == "Employees" && item.Item4==true)
                 {
                     Debug.WriteLine(item.Item2);
                 }

             }

             return;*/

            foreach (var node in union())
            {
                foreach (var item in BLScrapingTask.getInfos(node.Value))
                {                 
                    if(item.Item1.ToLower()== "Location map".ToLower())
                    {
                        Debug.WriteLine(item.Item2);
                    }
                }
            }

            Debug.WriteLine($"union had {union().Count()} elements");

        }


        [TestMethod()]
        public void asm()
        {

            var recs = Enumerable.Range(0, 4).Select(s => new { e = 4, name = "yass" });
            
            Assert.IsTrue(recs.Count() == 4,"nope");

            using (var sw = new StreamWriter(@"E:\TOOLS\scraper\tests.yass\blWorkspace\tex.csv"))
            using (var csvw = new CsvWriter(sw, System.Globalization.CultureInfo.InvariantCulture))
            {
                csvw.WriteRecords(recs);
            }

           

            using (var sr = new StreamReader(@"E:\TOOLS\scraper\tests.yass\blWorkspace\tex.csv"))
                using(var csvr = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
            {
                var i = csvr.GetRecords(recs.First().GetType()).Count();
                Assert.IsTrue(i==4,$"got {i} not 4");
            }
                


        }




















        [TestMethod()]
        public void testEnnumeratePages()
        {
            string urlwith4pg = @"https://www.businesslist.ph/category/industrial-premises"; //pc=4
            bool v =BLScrapingTask.EnumeratePages(urlwith4pg).Count() == 4;
            Assert.IsTrue(v, "nope");
        }



        public HtmlNode resolvePageTest(string link)
        {
            string restsFolder = @"E:\TOOLS\scraper\tests.yass\blWorkspace\tempTestsTargestPages";
            var res = BLScrapingTask.downloadOrRead(link, restsFolder);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res);
            return doc.DocumentNode;
        }


        public IEnumerable<KeyValuePair<string,HtmlNode>> union()
        {
            string t2 = @"E:\TOOLS\scraper\tests.yass\blWorkspace\tempTestsTargestPages";

            var t1 = @"E:\TOOLS\scraper\tests.yass\blWorkspace\companies-raw\html";

           return Directory.GetFiles(t1).Select(f =>
            {
                var res = File.ReadAllText(f);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);
                return  new KeyValuePair<string,HtmlNode>(f, doc.DocumentNode);
            });


        }


        [TestMethod()]
        public void testValidators()
        {

           
            string[] multiplePages = new string[] {
                @"https://www.businesslist.ph/category/retail-services" ,
                @"https://www.businesslist.ph/category/fire-safety-consultants" ,
               @"https://www.businesslist.ph/location/laoag" ,
            } ;
            string[] singlePage = new string[] {
                @"https://www.businesslist.ph/location/bayawan" ,
                @"https://www.businesslist.ph/location/escalante" ,
                @"https://www.businesslist.ph/category/after-school-programs" ,
                @"https://www.businesslist.ph/location/new-lucena" ,
            };
            string[]  emptyListing = new string[] {
                @"https://www.businesslist.ph/category/local-authorities" ,
                @"https://www.businesslist.ph/category/child-daycare-services" ,
                @"https://www.businesslist.ph/category/political" ,
            };
            string [] noneListings = new string[] {
                @"https://www.businesslist.ph/" ,
                @"https://www.businesslist.ph/browse-business-cities" ,
                @"https://www.businesslist.ph/company/304612/golden-haven",
            };

            string url1 = @"https://www.businesslist.ph/category/letting-agents"; //last page:6 elements count: 165
            string url2 = @" https://www.businesslist.ph/category/general-business"; //pc=607 cc=18209
            foreach (var item in noneListings)
            {
                Assert.IsFalse(BLScrapingTask.isBusinessesListings(resolvePageTest(item)), $"isBusinessesListings excpected true forr page {item}");
                Assert.IsFalse(BLScrapingTask.isNoneEmptyBusinessesListings(resolvePageTest(item)), $"isNoneEmptyBusinessesListings excpected true forr page {item}");
                Assert.IsFalse(BLScrapingTask.isMultiplePagesBusinessesListings(resolvePageTest(item)), $"isMultiplePagesBusinessesListings excpected true forr page {item}");
            }

            foreach (var item in emptyListing)
            {
                Assert.IsTrue(BLScrapingTask.isBusinessesListings(resolvePageTest(item)), $"isBusinessesListings excpected true forr page {item}");
                Assert.IsFalse(BLScrapingTask.isNoneEmptyBusinessesListings(resolvePageTest(item)), $"isNoneEmptyBusinessesListings excpected true forr page {item}");
                Assert.IsFalse(BLScrapingTask.isMultiplePagesBusinessesListings(resolvePageTest(item)), $"isMultiplePagesBusinessesListings excpected true forr page {item}");
            }

            foreach (var item in singlePage)
            {
                Assert.IsTrue(BLScrapingTask.isBusinessesListings(resolvePageTest(item)), $"isBusinessesListings excpected true forr page {item}");
                Assert.IsTrue(BLScrapingTask.isNoneEmptyBusinessesListings(resolvePageTest(item)), $"isNoneEmptyBusinessesListings excpected true forr page {item}");
                Assert.IsFalse(BLScrapingTask.isMultiplePagesBusinessesListings(resolvePageTest(item)), $"isMultiplePagesBusinessesListings excpected true forr page {item}");
            }

           

            foreach (var item in multiplePages)
            {
                Assert.IsTrue(BLScrapingTask.isBusinessesListings(resolvePageTest(item)), $"isBusinessesListings excpected true forr page {item}");
                Assert.IsTrue(BLScrapingTask.isNoneEmptyBusinessesListings(resolvePageTest(item)), $"isNoneEmptyBusinessesListings excpected true forr page {item}");
                Assert.IsTrue(BLScrapingTask.isMultiplePagesBusinessesListings(resolvePageTest(item)), $"isMultiplePagesBusinessesListings excpected true forr page {item}");
            }




            Assert.IsTrue(BLScrapingTask.getLastPageNumber(resolvePageTest(url1)) == 6);
            Assert.IsTrue(BLScrapingTask.getLastPageNumber(resolvePageTest(url2)) == 607);
        }

        //
        [TestMethod()]
        public void mutliplePages()
        {
            string url4 = @"https://www.businesslist.ph/category/industrial-premises";
            string rawPage = BLScrapingTask.downloadOrRead(url4, Workspace.Current.GetTPFolder());
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            string ResolvedTitle = BLScrapingTask.getPageTitle(doc.DocumentNode);
            var compactElements = BLScrapingTask.getCompactElementsInPage(doc.DocumentNode).ToList();
            Assert.IsTrue(compactElements.Count == 30, $"not 30 on page 1 but {compactElements.Count}");
            Assert.IsTrue(compactElements.All(e =>
            {
                return
                string.IsNullOrWhiteSpace(e.company) == false
                && string.IsNullOrWhiteSpace(e.link) == false
                ;
            }), "not all compacts on page1 have name and link");
        }




        [TestMethod()]
        public void parsng1()
        {
            string url1 = @"https://www.businesslist.ph/location/manila/3";
            string url2 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";
            string rawPage = BLScrapingTask. downloadOrRead(url1,Workspace.Current.GetTPFolder());
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            string ResolvedTitle = BLScrapingTask. getPageTitle(doc.DocumentNode);
            var compactElements = BLScrapingTask. getCompactElementsInPage(doc.DocumentNode).ToList();
            Assert.IsTrue(compactElements.Count == 30, $"not 30 on page 1 but {compactElements.Count}");
            Assert.IsTrue(compactElements.All(e =>
            {
                return
                string.IsNullOrWhiteSpace(e.company) == false
                && string.IsNullOrWhiteSpace(e.link) == false
                ;
            }), "not all compacts on page1 have name and link");
        }

        [TestMethod()]
        public void parsng2()
        {
            string testName = "page2";
            string url2 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";
            string rawPage = BLScrapingTask. downloadOrRead(url2, Workspace.Current.GetTPFolder());
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            string ResolvedTitle = BLScrapingTask. getPageTitle(doc.DocumentNode);
            var compactElements = BLScrapingTask. getCompactElementsInPage(doc.DocumentNode).ToList();
            Assert.IsTrue(compactElements.Count == 23, $"not 30 on {testName} but {compactElements.Count}");
            Assert.IsTrue(compactElements.All(e =>
            {
                return
                string.IsNullOrWhiteSpace(e.company) == false
                && string.IsNullOrWhiteSpace(e.link) == false
                ;
            }), $"not all compacts on {testName} have name and link");
        }



      
        

        [TestMethod()]
        public void BLScraperTests()
        {
            var ws = Workspace.GetWorkspace(ConfigService.Instance.WorkspaceDirectory);
            var p = new BLScraper() { WorkspaceDirectory=ConfigService.Instance.WorkspaceDirectory};
            var mvm = new MainViewModel(p, ws);
            string targetPageUrl = @"https://www.businesslist.ph/location/manila/3";
            mvm.DevFillAndStartCommand.Execute("4");
            
            mvm.scrapingTaskAwaiter.GetResult();
            //Task.Delay(50 * 300 + 1300 + 2000).GetAwaiter().GetResult();
           
           // Assert.IsTrue(o != null, "no content");
           // Assert.IsTrue(o.DownloadProgress.Current == 49, "the final download prog is not correct");


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