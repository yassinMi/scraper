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
            Debug.WriteLine($"number of items: {mvm.BusinessesViewModels.Count}");
            Assert.IsTrue(mvm.BusinessesViewModels.Count > 10);
        }

        [TestMethod()]
        public void asm()
        {

           
            

        }























        string getAddress(HtmlNode node)
        {
            var div = node.SelectSingleNode(".//div[@class='address']");
            if (div == null) return null;
            if (string.IsNullOrWhiteSpace(div.InnerHtml)) return null;
            return Utils.StripHTML(div.InnerHtml);
        }
        string getName(HtmlNode node)
        {
            return node.SelectSingleNode("./h4//a")?.InnerHtml;
        }
        string getLink(HtmlNode node)
        {
            return node.SelectSingleNode(".//h4//a")?.GetAttributeValue<string>("href", null);
        }
        string getDesc(HtmlNode node)
        {
            return "unresolved";
        }
        string getThumb(HtmlNode node)
        {
            var img = node.SelectSingleNode(".//div[@class='details']//a//span[@class='logo']//img");
            return (img != null) ? img.GetAttributeValue("src", "") : null;
        }


        IEnumerable<Business> getCompactElementsInPage(HtmlNode targetpagenode)
        {
            IEnumerable<HtmlNode> nodes = targetpagenode.Descendants(0)
             .Where(n => n.HasClass("company")&&!n.HasClass("company_ad"));
            
            foreach (HtmlNode node in nodes)
            {
                Debug.WriteLine(node.InnerHtml);
                var name = getName(node);
                if (name == null)
                {
                    Debug.WriteLine("wrong: name parsing");
                }
                var addr = getAddress(node);
                if (name == null)
                {
                    Debug.WriteLine("wrong: addr parsing");
                }
                var relativeLink = getLink(node);
                if (name == null)
                {
                    Debug.WriteLine("wrong: relativeLink parsing");
                }
                var thumb = getThumb(node);

                yield return new Business()
                {

                    name = name,
                    //desc = getDesc(node),
                    address = addr,
                    link = @"https://www.businesslist.ph" + relativeLink,
                    imageUrl = thumb,


                };
            }
        }


        /// <summary>
        /// only required by downloadOrReadFromObject, returns a filesystem-friendly hash
        /// </summary>
        string getUniqueLinkHash(string businessLink)
        {
            return Utils. CreateMD5(businessLink);
        }
        public string downloadOrReadFromTargetPageObject(string businessLink)
        {
            string blWSpath = @"E:\TOOLS\scraper\tests.yass\blWorkspace";

            Debug.WriteLine("calculating unique html name");
            string uniqueFilename = Path.Combine(blWSpath, ConfigService.Instance.TargetPagesRelativeLocation, getUniqueLinkHash(businessLink) + ".html");
            if (File.Exists(uniqueFilename))
            {
                //load
                Debug.WriteLine("exists");

                return File.ReadAllText(uniqueFilename);
            }
            else
            {
                //download
                Debug.WriteLine("new");
                string rawElementPage = WebHelper.instance.GetPageTextSync(businessLink);
                File.WriteAllText(uniqueFilename, rawElementPage);
                return rawElementPage;

            }

        }


        string getPageTitle(HtmlNode targetpagenode)
        {
            var h1 = targetpagenode.SelectSingleNode("//main//section//h1");
            return h1?.InnerHtml;
        }

        string getPageNumber(HtmlNode targetpagenode)
        {
            return "3";
        }


        [TestMethod()]
        public void parsng1()
        {
            string url1 = @"https://www.businesslist.ph/location/manila/3";
            string url2 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";
            string rawPage = downloadOrReadFromTargetPageObject(url1);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            string ResolvedTitle = getPageTitle(doc.DocumentNode);
            var compactElements = getCompactElementsInPage(doc.DocumentNode).ToList();
            Assert.IsTrue(compactElements.Count == 30, $"not 30 on page 1 but {compactElements.Count}");
            Assert.IsTrue(compactElements.All(e =>
            {
                return
                string.IsNullOrWhiteSpace(e.name) == false
                && string.IsNullOrWhiteSpace(e.link) == false
                ;
            }), "not all compacts on page1 have name and link");
        }

        [TestMethod()]
        public void parsng2()
        {
            string testName = "page2";
            string url2 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";
            string rawPage = downloadOrReadFromTargetPageObject(url2);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            string ResolvedTitle = getPageTitle(doc.DocumentNode);
            var compactElements = getCompactElementsInPage(doc.DocumentNode).ToList();
            Assert.IsTrue(compactElements.Count == 23, $"not 30 on {testName} but {compactElements.Count}");
            Assert.IsTrue(compactElements.All(e =>
            {
                return
                string.IsNullOrWhiteSpace(e.name) == false
                && string.IsNullOrWhiteSpace(e.link) == false
                ;
            }), $"not all compacts on {testName} have name and link");
        }



        [TestMethod()]
        public void downloadOrReadFromObjectTest()
        {
            string blWSpath = @"E:\TOOLS\scraper\tests.yass\blWorkspace";
            Workspace.MakeCurrent(blWSpath);
            BLScrapingTask bt = new BLScrapingTask() { WorkspaceDirectory = Workspace.Current.Directory};
           string s = bt.downloadOrReadFromObject(@"https://www.businesslist.ph/company/264051/maximo-a-lim-dental-clinic");

            Assert.IsTrue(s.Length == 78039, $"not the excpected ength but: {s.Length} ");

        }

        

        [TestMethod()]
        public void BLScraperTests()
        {
            var ws = Workspace.GetWorkspace(@"E:\TOOLS\scraper\tests.yass\blWorkspace");
            var p = new BLScraper();
            var mvm = new MainViewModel(p, ws);
            string targetPageUrl = @"https://www.businesslist.ph/location/manila/3";
            mvm.TargetPageQueryText = targetPageUrl;
            mvm.StartScrapingCommand.Execute(null);
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