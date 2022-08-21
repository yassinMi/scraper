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
using OpenQA.Selenium.Support.UI;

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

        public static void WaitFor(IWebDriver wd, By by, TimeSpan tm)
        {
            WebDriverWait w = new WebDriverWait(wd, tm);

            w.Until((iwd) =>
            {
                try
                {
                    return wd.FindElement(by) != null;
                }
                catch (StaleElementReferenceException err)
                {
                    throw;
                }
            });
        }

        public void ResolveElement(object compactElement, out int bytes, out int obj_cc)
        {
            bytes = 0;
            obj_cc = 0;
        }
        const string ELEMENT_CLASS = "_1hf7139";
        const string ELEMENT_CLASS_SELECTED = "_1uckoc70";
        const string ELEMENT_CLASS_HOVERED = "_oqztd3y"; //hovered not selected 
        const string ELEMENT_CLASS_HOVERED_SELECTED = "_19keelio"; //hovered not selected

        const string _elems_content_x = ".//div[@class='_1xzra4e']/div[@class='_1g0w9mx']/div[@class='_jcreqo']/div[@class='_1tdquig']/div[@class='_z72pvu']/div[@class='_3zzdxk']/div[@class='_1667t0u']/div[@class='_1rkbbi0x']/div[@class='_15gu4wr']";
        const string _details_section_x = ".//div[@class='_r47nf']//div[@class='_18lzknl']"; //works from doc level

        const string ELEMENTS_WRAPPER_CLASS = "_z72pvu";
        static WebDriver mainWebDriver { get; set; } = null;
        static Synchronizer<string> _lock = new Synchronizer<string>();
        bool titleHasBeenResolved = false;

        public override async Task RunScraper(CancellationToken ct)
        {
            CoreUtils.WriteLine($"RunScraper.. [{DateTime.Now}], {TargetPage}");

            lock (_lock[TargetPage])
            {
                List<Company> list = new List<Company>();
                if (Workspace.Current?.CSVOutputFolder == null) Debug.WriteLine("null ws");
                IWebElement list__; //last wrapper that ahs divs directly
                IWebElement list_wrapper_rnd; // a common wrapper for pagnation also;
                OnTaskDetailChanged("Waiting for other task(s) to end..");
                lock (_lock)//concurrent tasks can'y run this part of code simultaneously
                {

                    if (mainWebDriver == null)
                    {
                        OnStageChanged(ScrapTaskStage.Setup);
                        OnTaskDetailChanged("Starting chrome..");
                        CoreUtils.WriteLine("Starting driver..");
                        try
                        {
                            mainWebDriver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), new ChromeOptions(),TimeSpan.FromMinutes(3));
                        }
                        catch (DriverServiceNotFoundException err)
                        {
                            CoreUtils.WriteLine($"couldn't start chrome: {err}");
                            OnError(err.Message);
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                        catch (Exception err)
                        {
                            CoreUtils.WriteLine($"couldn't start chrome:unknown error {err}");
                            OnError(err.Message);
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                        
                    }

                    string cachedHtmlFilename = Path.Combine(Workspace.Current.TPFolder, CoreUtils.getUniqueLinkHash(TargetPage)+".html");
                    string cacheUrlOrOriginalUrl = TargetPage;
                    bool enable_cache = false; //obsolete
                    bool shouldBeCached = true; // ndicating the target page must be cached as soon as fetched successfully.
                    if (enable_cache && File.Exists(cachedHtmlFilename))
                    {
                        cacheUrlOrOriginalUrl = cachedHtmlFilename;
                        shouldBeCached = false;
                    }
                    Debug.WriteLine("url driver");
                    
                    OnStageChanged(ScrapTaskStage.DownloadingData);
                    Debug.WriteLine("nav");
                    try
                    {
                        mainWebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                        CoreUtils.WriteLine("SwitchTo() ..");
                        mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                        OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                        mainWebDriver.Url = cacheUrlOrOriginalUrl;
                       CoreUtils.WriteLine("Navigate()..");
                        mainWebDriver.Navigate();
                    }
                    catch (Exception err)
                    {
                        CoreUtils.WriteLine($"couldnt navigate :unknown error {err}");
                        OnError(err.Message);
                        OnStageChanged(ScrapTaskStage.Failed);
                        return;
                    }
                    bool should_scrape_next_page = false;
                    int current_page = tryGetPageNumFromUrl(TargetPage);
                    do
                    {
                        try
                        {

                            OnTaskDetailChanged("waiting for elemetns_content");
                            WebDriverWait w = new WebDriverWait(mainWebDriver, TimeSpan.FromSeconds(30));
                            w.Until((e) =>
                            {
                                try
                                {
                                    return e.FindElement(By.XPath(".//div[@class='_z72pvu']//div[@class='_1667t0u']//div[@class='_awwm2v']")) != null;
                                }
                                catch (StaleElementReferenceException err)
                                {
                                    return false;
                                }
                                catch (NoSuchElementException err)
                                {
                                    return false;
                                }
                                catch (Exception err)
                                {
                                    CoreUtils.WriteLine($"Until: unknown error {err}");
                                    return false;
                                }
                            });
                            if (titleHasBeenResolved == false)
                            {
                                titleHasBeenResolved = true;
                                OnResolved(mainWebDriver.Title ?? "title error");
                                ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, CoreUtils.SanitizeFileName(mainWebDriver.Title) + ".csv");

                            }
                            if (shouldBeCached)
                            {
                                File.WriteAllText(cachedHtmlFilename, mainWebDriver.PageSource);
                                Debug.WriteLine($"saved cache tp file at: '{cachedHtmlFilename}' ");
                            }
                            Debug.WriteLine("getting categories_welements_wrapper..");
                            //# listing
                            //elems_content is _z72pvu ([2nd]after filr) o>  _3zzdxk o> _1667t0u o> _1rkbbi0x o> _15gu4wr 
                            //list is: div[2rd] no class o> _awwm2v
                            //currentlyusing .//div[@class='_z72pvu']//div[@class='_1667t0u']
                            list_wrapper_rnd = mainWebDriver.FindElement(
                                By.XPath(".//div[@class='_z72pvu']//div[@class='_1667t0u']"));
                            list__ = list_wrapper_rnd.FindElement(By.XPath(".//div[@class='_awwm2v']"));
                            //var scrollView = list_wrapper_rnd.FindElement(By.ClassName("_1rkbbi0x"));
                            //mainWebDriver.ExecuteScript("document.getElementsByClassName(\"_1rkbbi0x\")[2].scrollTo(0,50000)");

                        }
                        catch (Exception err)
                        {

                            OnError(err.Message);
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }

                        tryHideFooter();
                        // By.XPath("/*[@class='Sidebar__Container-gs0c67-0 bXQeSB sidebar']"));
                        Debug.WriteLine($"enumerating categories_welements.. is null {list__.GetDomProperty("innerHTML")}");

                        var elements_divs = list__.FindElements(By.XPath($"./div"));
                        Debug.WriteLine($"elements_divs null {elements_divs == null} ");
                        Debug.WriteLine($"elements_divs count {elements_divs.Count} ");


                        int i = 0;
                        CoreUtils.WriteLine($"page started:{current_page}");
                        OnPageStarted($"page {current_page}");
#if true


                        foreach (var div in elements_divs)
                        {
                            var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                            try
                            {
                                act.ScrollToElement(elements_divs[Math.Min(i + 1, elements_divs.Count - 1)]);
                                act.Perform();
                            }
                            catch (Exception err)
                            {
                                CoreUtils.WriteLine($"act.Perform failed: {err}");
                                Trace.Fail("act.Perform failed", err.ToString());
                            }
                            Task.Delay(80).GetAwaiter().GetResult();
                            i++;
                            Company new_cmp_elem = new Company();
                            Debug.WriteLine($"creating new comp_elem ..");
                            var element_component = div.FindElement(By.XPath("./div"));
                            new_cmp_elem.companyName = getName(element_component);
                            OnTaskDetailChanged($"{new_cmp_elem.companyName}/location");
                            new_cmp_elem.location = getLocation(element_component);
                            OnTaskDetailChanged($"{new_cmp_elem.companyName}/category");
                            new_cmp_elem.category = getCategory(element_component);
                            OnTaskDetailChanged($"{new_cmp_elem.companyName}/link");
                            new_cmp_elem.link = getLink(element_component);
                            ResolveElementDynamic2(new_cmp_elem, element_component);
                            TaskStatsInfo.incElem(1);
                            Debug.WriteLine($"delaying ..");
                            Task.Delay(20).GetAwaiter().GetResult();
                            list.Add(new_cmp_elem);
                            OnProgress(new DownloadingProg() { Current = i, Total = elements_divs.Count });
                            if (ct.IsCancellationRequested) {
                                if (IsStopRequested == false)
                                {
                                    OnIsStopRequestedChanged(true);
                                    CoreUtils.WriteLine($"canceled at element:'{i}', page:'{current_page}'");
                                }
                                break;
                            }
                        }
                        CSVUtils.CSVWriteRecords(ActualOutputFile, list, false);
#endif
                        IWebElement next=null, prev;
                        IWebElement[] pages_butts=null;
                        bool isNextEnabled=false, exists_next_page=false;
                        int curr_page_num=0;
                        exists_next_page = resolvePagination(list_wrapper_rnd, out pages_butts, out next, out prev, out isNextEnabled, out curr_page_num) && isNextEnabled;
                        CoreUtils.WriteLine($"exists_next_page result : {should_scrape_next_page}, curr_page_num:'{curr_page_num}',isNextEnabled:'{isNextEnabled}',pages_buttons:'{ (pages_butts == null ? "null" : string.Join(",", pages_butts.Select(b => b.Text)))}'");

                        if (ct.IsCancellationRequested)
                        {
                            if (IsStopRequested == false)
                            {
                                OnIsStopRequestedChanged(true);
                                CoreUtils.WriteLine($"canceled at page {current_page}");
                            }
                            should_scrape_next_page = false;

                        }
                        else
                        {
                            should_scrape_next_page = exists_next_page;
                        }
                        Debug.WriteLine($"at page {current_page} calling it.");

                        if (should_scrape_next_page)
                        {
                            //#clicking next page
                            try
                            {
                                next?.Click();
                                Task.Delay(100).GetAwaiter().GetResult();
                            }
                            catch (Exception err)
                            {
                                CoreUtils.WriteLine($"next.Click(): {err}");
                                Trace.Fail("next failed", err.ToString());
                            }
                        }
                        if(current_page != curr_page_num)
                        {
                            CoreUtils.WriteLine($"Expected current page num to be '{current_page}', parser returned '{curr_page_num}'");
                            Trace.Fail( $"Expected current page num to be '{current_page}', parser returned '{curr_page_num}'");
                        }
                        current_page++;

                    } while (should_scrape_next_page);

                    
                }
                    

                OnStageChanged(ScrapTaskStage.Success);
                return;

            }
        }
        /// <summary>
        /// excractes page from url, returns 1 as fallback
        /// </summary>
        /// <param name="targetPage"></param>
        /// <returns></returns>
        public static int tryGetPageNumFromUrl(string targetPage)
        {
            var m = Regex.Match(targetPage, @"/page/(\d+)");
            if (m.Success)
            {
                try
                {
                    return int.Parse(m.Groups[1].Value);
                }
                catch (Exception)
                {

                    return 1;
                }
                
            }
            return 1;
        }

        /// <summary>
        /// footer sometimes receves pagination click events and causes problems
        /// this methods attempts to clos it if exists)
        /// </summary>
        private void tryHideFooter()
        {
            var footer_close_button_x_ = "//footer//div[@class='_euwdl0']";
            try
            {
                var butt = mainWebDriver.FindElement(By.XPath(footer_close_button_x_));
                butt.Click();

            }
            catch(NoSuchElementException)
            {
                CoreUtils.WriteLine($"tryHideFooter: NoSuchElementException");

                return;
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"tryHideFooter: unknown axception : {err}");
                return;
            }
        }

        const string RootInfo_InElementPage = "_1rkbbi0x";
        [Obsolete("use 2", true)]
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
        /// <summary>
        /// es
        /// no tab apprach, clicks on element_component and waits for details_section 
        /// </summary>
        /// <param name="compact_elem"></param>
        /// <param name="element_component">element_component</param>
        private void ResolveElementDynamic2(Company compact_elem, IWebElement element_component)
        {
            if (element_component == null)
            {
                CoreUtils.WriteLine($"ResolveElementDynamic2: null obj passed");
                return;
            }
            string elem_calss = element_component.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"ResolveElementDynamic2: expected one of element_component classes got {elem_calss}");
                return;//todo should return only in a strict_dev_mode, otherwise keep going as long as exception safe
            }
            try
            {
                OnTaskDetailChanged($"{compact_elem.companyName}/waiting for phone info");
                element_component.Click();
                WaitFor(mainWebDriver, By.XPath(_details_section_x + "//div[@class='_599hh']"), TimeSpan.FromSeconds(8));
                var tel_awaiter = By.XPath(".//div[@class='_49kxlr']/div[@class='_b0ke8']/a[@class='_2lcm958']");
                WaitFor(mainWebDriver, tel_awaiter, TimeSpan.FromSeconds(20));
                //#expanding phones and joining all:  //not supported
                OnTaskDetailChanged($"{compact_elem.companyName}/delay 10ms");
                Task.Delay(10).GetAwaiter().GetResult();
                OnTaskDetailChanged($"{compact_elem.companyName}/phone");
                compact_elem.phone = getPhone(mainWebDriver.FindElement(By.XPath(_details_section_x)));

            }
            catch  (WebDriverTimeoutException err)
            {
                CoreUtils.WriteLine($"ResolveElementDynamic: timed out after 20 seconds, name: '{compact_elem.companyName}', link:'{compact_elem.link}, page: '{Page}'");
                return;
            }
            catch (Exception err)
            {

                CoreUtils.WriteLine($"ResolveElementDynamic: (name: '{compact_elem.companyName}', link:'{compact_elem.link}, page: '{Page}'), unkown exception {err}");
                return;
            }
             }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">element_component</param>
        /// <returns></returns>
        private string getLink(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getLink: passed null object");
                return "N/A";
            }

            Debug.WriteLine($"link ..");
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"getLink: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            try
            {
                string lon= item.FindElement(By.XPath("./div[2]/a"))?.GetAttribute("href") ?? "N/A";
                return lon.Split('?').FirstOrDefault()??"N/A";
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getLink: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getLink: unknows exception: {err}");
                return "N/A";
            }
        }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">details section </param>
        /// <returns></returns>
        public  string getPhone(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getPhone: passed null object");
                return "N/A";
            }
            //div _49kxlr >  div _b0ke8 > a _2lcm958  has format tel:+97142398771 in href
            Debug.WriteLine($"phone ..");
            try
            {
                var fe = item.FindElements(By.XPath(".//div[@class='_49kxlr']/div[@class='_b0ke8']"));///a[@class='_2lcm958']
                if (fe == null) return "N/A";
                if (fe.Any() == false) return "N/A";
                IWebElement show_button;
                IWebElement first_div = null;
                try
                {
                    first_div = fe.First();              
                    show_button = first_div.FindElement(By.XPath("./button"));
                    var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                    try
                    {
                        act.ScrollToElement(show_button);
                        act.Perform();
                    }
                    catch (Exception err)
                    {
                        CoreUtils.WriteLine($"act.Perform failed: {err}");
                    }
                    //#clicking
                    Task.Delay(80).GetAwaiter().GetResult();
                    show_button.Click();
                    Task.Delay(10).GetAwaiter().GetResult();
                }
                catch (ElementNotInteractableException err)
                {
                    CoreUtils.WriteLine($"getPhone: ElementNotInteractableException: (page: '{Page}') innerHTML: {first_div?.GetAttribute("innerHTML")}, {err}");
                }
                catch (Exception err)
                {
                    CoreUtils.WriteLine($"getPhone: exc: {err}");
                    //bkc_up_return
                    
                }
                
                var ee = item.FindElements(By.XPath(".//div[@class='_49kxlr']/div[@class='_b0ke8']/a[@class='_2lcm958']"));///
                if (ee == null) return "N/A";
                if (ee.Any() == false) return "N/A";
                return string.Join(" / ", ee.Select(a => a?.GetAttribute("href")));
                
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getPhone: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getPhone: unknown exception: {err}");
                return "N/A";
            }
        }

        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">element_component</param>
        /// <returns></returns>
        private string getCategory(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getCategory: passed null object");
                return "N/A";
            }
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"getCategory: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            Debug.WriteLine($"cat ..");
            try
            {
                return getElementText(item.FindElement(By.XPath("./div[3]")))?? "N/A";
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getCategory: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getCategory: unknown exception: {err}");
                return "N/A";
            }


        }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">element_component</param>
        /// <returns></returns>
        private string getLocation(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getLocation: passed null object");
                return "N/A";
            }
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"getLocation: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            //the 4th div (not always,) or the class _4l12l8 (unless it's grayed out
            //here it's class _15orusq2 )
            Debug.WriteLine($"loc ..");
            try
            {
                var case_normal = item.FindElements(By.XPath("./div[@class='_4l12l8']//span[@class='_1w9o2igt']"));
                if (case_normal.Count > 0) {
                    if (case_normal.FirstOrDefault() == null) return "N/A";
                    string s= getElementText(case_normal.FirstOrDefault());
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        CoreUtils.WriteLine($"getLocation; empty name at item: (title item: ({case_normal.FirstOrDefault().GetAttribute("innerHTML")}){Environment.NewLine}");
                    }
                    return s;
                } 
                else
                {
                    var case_gray = item.FindElements(By.XPath("./div[@class='_15orusq2']"));//todo add //span[@class='_1w9o2igt'] is it exists to avoid branches
                    if (case_gray.Count > 0)
                    {
                        if (case_gray.FirstOrDefault() == null) return "N/A";
                        string s = getElementText(case_gray.FirstOrDefault());
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            CoreUtils.WriteLine($"getLocation: case_gray empty name at item: (title item: ({case_gray.FirstOrDefault().GetAttribute("innerHTML")}){Environment.NewLine}");
                        }
                        return s;
                    }
                    
                    else
                    {
                        CoreUtils.WriteLine("getLocation: counld'd find loc in 2 cases");
                        return "N/A";
                    }
                }

            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getLocation: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getLocation: unknown exception: {err}");
                return "N/A";
            }
        }

        public static string getElementText(IWebElement d)
        {
            if (d == null) return null;
            string s = d.Text;
            if (string.IsNullOrWhiteSpace(s))
            {
                s = d.GetAttribute("innerText");
            }
            if (string.IsNullOrWhiteSpace(s))
            {
                s = d.GetAttribute("textContent");
            }
            return s;
        }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">element_cmomponent</param>
        /// <returns></returns>
        private string getName(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getName: passed null object");
                return "N/A";
            }
            Debug.WriteLine($"name ..");
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"getName: expected one of element_cmomponent classes got {elem_calss}");
                return "N/A";
            }
            try
            {
                var d = item.FindElement(By.XPath("./div[2]"));
                if (d == null) return "N/A";
                string inner = d?.GetAttribute("innerHTML");
                string s = getElementText(d);
                if (string.IsNullOrWhiteSpace(s))
                {
                    CoreUtils.WriteLine($"getName; empty name at item: (title item: ({inner}){Environment.NewLine}");
                }
                return s;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("getName: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getName: unknown exception: {err}");
                return "N/A";
            }


        }
        /// <summary>
        /// es
        /// retirns a value indicating wither the pagination footer exists
        /// </summary>
        /// <param name="elems_content"></param>
        /// <param name="pagesButtons">clickable a elements, the selected one is excluded</param>
        /// <param name="next"></param>
        /// <param name="prev"></param>
        /// <returns>false means single page or something wrong (tarce)</returns>
        bool resolvePagination(IWebElement elems_content, out IWebElement[] pagesButtons, out IWebElement next, out IWebElement prev, out bool isNextEnabled, out int curr_page_num)
        {
            var enabled_nav_butt_class_ = "_n5hmn94";
            var disabled_nav_butt_class_ = "_7q94tr";
            var display_non_class_ = "_fe2hl4";//used for 3rd div in case no pagination section 
            var normal_pagination_class_ = "_1x4k6z7"; //in normal pagination
            var pagination_x_ = ".//div[@class='_15gu4wr']/div[3]";//from elems_content or before
            var pages_x_ = "./div[1]/*";//from pagination
            IWebElement pagination;
            try
            {
                
                pagination = elems_content.FindElement(By.XPath(pagination_x_));
                var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                act.ScrollToElement(pagination);
                act.Perform();
                if (pagination.GetAttribute("class")== "_1x4k6z7")
                {
                    var all_pages = pagination.FindElements(By.XPath(pages_x_));//including the selected one (whch is a div and not a)
                    pagesButtons = all_pages.Where(e => e.TagName == "a").ToArray();
                    Debug.WriteLine($"all pages are {string.Join(", ", all_pages.Select(p=>p.Text))} .");

                    var nav_next = pagination.FindElement(By.XPath(".//div[@class='_5ocwns']/div[2]"));
                    var nav_prev = pagination.FindElement(By.XPath(".//div[@class='_5ocwns']/div[1]"));
                    next = nav_next; prev = nav_prev;
                    isNextEnabled = next.GetAttribute("class") == enabled_nav_butt_class_;
                    string te = all_pages.First((e) => e.TagName == "div" && e.GetAttribute("class")== "_l934xo5").Text;
                    
                    if( int.TryParse(te,out curr_page_num) == false)
                    {
                        CoreUtils.WriteLine($"expected number got {te}");
                    };
                    return true;
                }
                else
                {
                    //pagination is hidden, page would be 1
                    CoreUtils.WriteLine($"else cl .");
                    pagesButtons = null; prev = null; next = null; isNextEnabled = false;
                    curr_page_num = 1;
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine($"no such.");

                CoreUtils.WriteLine($"resolvePagination:warning: NoSuchElementException");
                pagesButtons = null; prev = null; next = null; isNextEnabled = false;
                curr_page_num = 0;
                return false;

            }
            catch (Exception err)
            {
                Debug.WriteLine($"gen exc");

                CoreUtils.WriteLine($"resolvePagination:nkow axception {err}");
                pagesButtons = null; prev = null; next = null; isNextEnabled = false;
                curr_page_num = 0;
                return false;

            }

        }

        public override Task RunConverter()
        {
            throw new NotImplementedException();
        }
    }
}
