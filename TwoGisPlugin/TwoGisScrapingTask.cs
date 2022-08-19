using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using TwoGisPlugin.Model;
using System.IO;
using OpenQA.Selenium.Chrome;

namespace TwoGisPlugin
{
    public class TwoGisScrapingTask : ScrapingTaskBase
    {
        public TwoGisScrapingTask(string tp)
        {
            TargetPage = tp;
        }
        public  IEnumerable<T> EnumerateCompactElements<T>(HtmlNode pageNode)
        {
            yield break;
        }

        public  IEnumerable<Tuple<int, int, HtmlNode>> EnumeratePages_static(string rootPageUrl)
        {
            Debug.WriteLine($"EnumeratePages ");

            string pageRaw = downloadOrRead(rootPageUrl, Workspace.Current.TPFolder);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageRaw);
            Debug.WriteLine($"loaded html doc ");

            if ((isBusinessesListings(doc.DocumentNode) == false) || (isNoneEmptyBusinessesListings(doc.DocumentNode) == false))
            {
                yield break;
            }
            int min = 1;
            int max = 1;
            Debug.WriteLine($"is null {doc.DocumentNode==null}");
            if (isMultiplePagesBusinessesListings(doc.DocumentNode))
            {
                max = getLastPageNumber(doc.DocumentNode);
            }
            Debug.WriteLine($"foreach range");
            foreach (var pg in Enumerable.Range(min, max))
            {
                string pageLink = AppendPageNumToUrl(ClearPageNumFromUrl(rootPageUrl), pg);
                string raw = downloadOrRead(pageLink, Workspace.Current.TPFolder);
                HtmlDocument newDoc = new HtmlDocument();
                newDoc.LoadHtml(raw);
                Debug.WriteLine($"y return page tuple");
                yield return new Tuple<int, int, HtmlNode>(pg, max, newDoc.DocumentNode);
            }


        }

        public IEnumerable<Tuple<int, int, HtmlNode>> EnumeratePages(string rootPageUrl)
        {
            yield break;

        }

        public static string downloadOrRead(string pageLink, string folder)
        {
            Debug.WriteLine($"downloadOrRead '{pageLink}'");
            string uniqueFilename = Path.Combine(folder, CoreUtils.getUniqueLinkHash(pageLink) + ".html");
            if (File.Exists(uniqueFilename)) { return File.ReadAllText(uniqueFilename); }
            else
            {
                try
                {
                    string rawElementPage = WebHelper.instance.GetPageTextSync(pageLink);
                    File.WriteAllText(uniqueFilename, rawElementPage);
                    return rawElementPage;
                }
                catch (Exception err)
                {
                    Debug.WriteLine($"downloadOrRead throw {err.Message}");
                    throw;
                }

            }
        }


        public static int getLastPageNumber(HtmlNode pagenode)
        {
            Debug.WriteLine($"getLastPageNumber");

            var paegs_container = pagenode.SelectSingleNode("//div[@class='_1x4k6z7']");

            if (paegs_container==null )
            {
                throw new Exception("pages_container not found");
            }
            Debug.WriteLine($"paegs_container");

            var a_elems = paegs_container.SelectNodes("//a[@class='_12164l30']");

            var lasPafeNo = a_elems.LastOrDefault();
            if (lasPafeNo == null) throw new Exception("error while paring getLastPageNumber");
            int res = int.Parse(lasPafeNo.InnerText);
            if (res <  1) throw new Exception("getLastPageNumber: number cannot be <1");
            return res;
        }
        public static string AppendPageNumToUrl(string url, int pageNum)
        {
            return url.TrimEnd(new char[] { '/' }) + "/page/" + pageNum.ToString();
        }

        public static string ClearPageNumFromUrl(string url)
        {
            return Regex.Replace(url, @"/page/\d+$|/page/\d+/$", "");
        }

        private bool isMultiplePagesBusinessesListings(HtmlNode documentNode)
        {
            return true;
        }

        private bool isNoneEmptyBusinessesListings(HtmlNode documentNode)
        {
            return true;
        }

        private bool isBusinessesListings(HtmlNode documentNode)
        {
            return true;
        }

        public string GetElementUniqueID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public string GetElementUserID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public string GetElementUserUniqueID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public bool HasElementsTBF(HtmlNode pageNode)
        {
            return isBusinessesListings(pageNode) && isNoneEmptyBusinessesListings(pageNode);
        }

        public override void Pause()
        {
            
        }

        public void ResolveElement(object compactElement, out int bytes, out int obj_cc)
        {
            bytes = 0;
            obj_cc = 0;
        }
        const string ELEMENT_CLASS = "_1hf7139"; 
             const string ELEMENTS_WRAPPER_CLASS = "_z72pvu";
        static WebDriver mainWebDriver { get; set; } = null;
        static Synchronizer<string> _lock = new Synchronizer<string>();

        public override async Task RunScraper(CancellationToken ct)
        {
            Debug.WriteLine("RunScraper call..");

            lock (_lock[TargetPage])
            {
                List<Company> list = new List<Company>();
                if (Workspace.Current?.CSVOutputFolder == null) Debug.WriteLine("null ws");
                IWebElement elements_wrapper;
                OnTaskDetailChanged("Waiting for other task(s) to end..");
                lock (_lock)//concurrent tasks can'y run this part of code simultaneously
                {

                    if (mainWebDriver == null)
                    {
                        OnStageChanged(ScrapTaskStage.Setup);
                        OnTaskDetailChanged("Starting chrome driver..");
                        mainWebDriver = new ChromeDriver();
                    }

                    mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                    OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                    Debug.WriteLine("GoToUrl driver..");
                    string cachedHtmlFilename = Path.Combine(Workspace.Current.TPFolder, CoreUtils.getUniqueLinkHash(TargetPage)+".html");
                    string cacheUrlOrOriginalUrl = TargetPage;
                    bool shouldBeCached = true; // ndicating the target page must be cached as soon as fetched successfully.
                    if (File.Exists(cachedHtmlFilename))
                    {
                        cacheUrlOrOriginalUrl = cachedHtmlFilename;
                        shouldBeCached = false;
                    }

                    mainWebDriver.Url = cacheUrlOrOriginalUrl;
                    OnStageChanged(ScrapTaskStage.DownloadingData);
                    try
                    {
                        mainWebDriver.Navigate();
                        OpenQA.Selenium.Support.UI.WebDriverWait w = new OpenQA.Selenium.Support.UI.WebDriverWait(mainWebDriver, TimeSpan.FromSeconds(30));

                        w.Until((e) =>
                        {
                            try
                            {
                                return e.FindElement(By.ClassName(ELEMENTS_WRAPPER_CLASS)) != null;
                            }
                            catch (StaleElementReferenceException err)
                            {
                                throw;
                            }
                        });
                        OnResolved(mainWebDriver.Title);
                        ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, CoreUtils.SanitizeFileName(mainWebDriver.Title) + ".csv");
                        if (shouldBeCached)
                        {
                            File.WriteAllText(cachedHtmlFilename, mainWebDriver.PageSource);
                            Debug.WriteLine($"saved cache tp file at: '{cachedHtmlFilename}' ");
                        }
                        Debug.WriteLine("getting categories_welements_wrapper..");
                        elements_wrapper = mainWebDriver.FindElement(
                            By.ClassName(ELEMENTS_WRAPPER_CLASS));
                        var scrollView = elements_wrapper.FindElement(By.ClassName("_1rkbbi0x"));
                        mainWebDriver.ExecuteScript("document.getElementsByClassName(\"_1rkbbi0x\")[2].scrollTo(0,50000)");
                        
                    }
                    catch (Exception err)
                    {

                        OnError(err.Message);
                        OnStageChanged(ScrapTaskStage.Failed);
                        return;
                    }

                    // By.XPath("/*[@class='Sidebar__Container-gs0c67-0 bXQeSB sidebar']"));
                    Debug.WriteLine($"enumerating categories_welements.. is null {elements_wrapper.GetDomProperty("innerHTML")}");

                    var elements_divs = elements_wrapper.FindElements(By.XPath($".//div[@class='{ELEMENT_CLASS}']"));
                    Debug.WriteLine($"elements_divs null {elements_divs==null} ");
                    Debug.WriteLine($"elements_divs count {elements_divs.Count} ");
                    
                    
                    int i = 0;
                    
                    OnPageStarted("test cat");

                    foreach (var item in elements_divs)
                    {
                        var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                        act.ScrollToElement(elements_divs[Math.Min( i+1, elements_divs.Count-1)]);
                        act.Perform();
                        Task.Delay(80).GetAwaiter().GetResult();
                        Debug.WriteLine($"enumerating topics in {"test cat"}..");
                        i++;
                        Company new_cmp_elem = new Company();
                        Debug.WriteLine($"creating new comp_elem ..");
                        new_cmp_elem.companyName = getName(item);
                        new_cmp_elem.phone = getPhone(item);
                        new_cmp_elem.location = getLocation(item);
                        new_cmp_elem.category = getCategory(item);
                        new_cmp_elem.link = getLink(item);
                        ResolveElementDynamic(new_cmp_elem);
                        TaskStatsInfo.incElem(1);
                        Debug.WriteLine($"delaying ..");
                        OnStageChanged(ScrapTaskStage.Delaying);
                        Task.Delay(500).GetAwaiter().GetResult();
                        list.Add(new_cmp_elem);
                        OnProgress(new DownloadingProg() { Current = i, Total = elements_divs.Count });


                    }
                }

                CSVUtils.CSVWriteRecords(ActualOutputFile, list, false);
                OnStageChanged(ScrapTaskStage.Success);
                return;

            }
        }
        const string RootInfo_InElementPage = "_1rkbbi0x";
        /// <summary>
        /// opens new tab and collects additional fields and switchs back to original tab
        /// </summary>
        /// <param name="new_cmp_elem"></param>
        private void ResolveElementDynamic(Company compact_elem)
        {
            //phone 
            string elem_link = compact_elem.link;
            string original_tab_handle = mainWebDriver.CurrentWindowHandle;
            elem_link = @"file:///F:/epicMyth-tmp-6-2022/freelancing/projects/gis/Al Falaq Technical Services Company, Al Rashdaan Building, 67, Al Muteena Street, Dubai — 2GIS.html";
            mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
            mainWebDriver.Url = elem_link;
            mainWebDriver.Navigate();
            compact_elem.phone = getPhone(mainWebDriver.FindElement(By.ClassName(RootInfo_InElementPage)));
            mainWebDriver.Close();
            mainWebDriver.SwitchTo().Window(original_tab_handle);
        }

        private string getLink(IWebElement item)
        {
            Debug.WriteLine($"link ..");
            Debug.Assert(item.GetAttribute("class") == ELEMENT_CLASS, "eexpected element class const");
            try
            {
                string res = item.FindElement(By.XPath("./div[2]/a"))?.GetAttribute("href") ?? "N/A";
                Debug.WriteLine($"res {res}");
                return res;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("element not found n returning n/a");
                return "N/A";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">a root of infos in element page</param>
        /// <returns></returns>
        public static string getPhone(IWebElement item)
        {
            //div _49kxlr >  div _b0ke8 > a _2lcm958  has format tel:+97142398771 in href
            Debug.WriteLine($"phone ..");
            try
            {
                var ee = item.FindElement(By.XPath(".//div[@class='_49kxlr']/div[@class='_b0ke8']/a[@class='_2lcm958']"));
                if (ee == null) return "N/A";
                return ee?.GetAttribute("href") ?? "N/A";
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("element not found n returning n/a");
                return "N/A";
            }
        }

        private string getCategory(IWebElement item)
        {
            Debug.WriteLine($"cat ..");
            try
            {
                return item.FindElement(By.XPath("./div[3]"))?.Text ?? "N/A";
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("element not found n returning n/a");
                return "N/A";
            }
            

        }

        private string getLocation(IWebElement item)
        {
            //the 4th div (not always,) or the class _4l12l8 (unless it's grayed out
            //here it's class _15orusq2 )
            Debug.WriteLine($"loc ..");
            try
            {
                var case_normal = item.FindElements(By.XPath("./div[@class='_4l12l8']"));
                if(case_normal.Count>0) return case_normal.FirstOrDefault()?.Text??"N/A";
                else
                {
                    var case_gray = item.FindElements(By.XPath("./div[@class='_15orusq2']"));
                    if (case_gray.Count > 0) return case_gray.FirstOrDefault()?.Text ?? "N/A";
                    else
                    {
                        Debug.WriteLine("element not found 5n returning n/a");
                        return "N/A";
                    }

                }

            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("element not found n returning n/a");
                return "N/A";
            }
        }

        private string getName(IWebElement item)
        {
            Debug.WriteLine($"name ..");
            Debug.Assert(item.GetAttribute("class") == ELEMENT_CLASS,"eexpected element class const");
            try
            {
                string res = item.FindElement(By.XPath("./div[2]"))?.Text ?? "N/A";
                Debug.WriteLine($"res {res}");
                return res;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("element not found n returning n/a");
                return "N/A";
            }
            

        }

        public override Task RunConverter()
        {
            throw new NotImplementedException();
        }
    }
}
