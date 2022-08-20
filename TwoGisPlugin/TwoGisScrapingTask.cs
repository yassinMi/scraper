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
            Debug.WriteLine("RunScraper call..");

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
                        mainWebDriver = new ChromeDriver();
                    }
                   
                    Debug.WriteLine("GoToUrl driver..");
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
                        mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                        OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                        mainWebDriver.Url = cacheUrlOrOriginalUrl;
                        mainWebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                        Debug.WriteLine("navigating..");
                        mainWebDriver.Navigate();
                    }
                    catch (Exception err)
                    {
                        OnError(err.Message);
                        OnStageChanged(ScrapTaskStage.Failed);
                        return;
                    }
                    bool exists_next_page = false;
                    int current_page = 1;
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
                        
                        OnPageStarted($"page {current_page}");

                        foreach (var div in elements_divs)
                        {
                            var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                            act.ScrollToElement(elements_divs[Math.Min(i + 1, elements_divs.Count - 1)]);
                            act.Perform();
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
                            OnStageChanged(ScrapTaskStage.Delaying);
                            Task.Delay(500).GetAwaiter().GetResult();
                            list.Add(new_cmp_elem);
                            OnProgress(new DownloadingProg() { Current = i, Total = elements_divs.Count });
                        }
                        CSVUtils.CSVWriteRecords(ActualOutputFile, list, false);

                        IWebElement next, prev;
                        IWebElement[] pages_butts;
                        bool isNextEnabled;
                        int curr_page_num;
                        exists_next_page = resolvePagination(list_wrapper_rnd, out pages_butts, out next, out prev, out  isNextEnabled, out curr_page_num) && isNextEnabled && !ct.IsCancellationRequested ;
                        if (exists_next_page)
                        {
                            //#clicking next page
                            next.Click();
                            Task.Delay(100).GetAwaiter().GetResult();
                        }
                        current_page = curr_page_num + 1;
                    } while (exists_next_page);

                    
                }
                    

                OnStageChanged(ScrapTaskStage.Success);
                return;

            }
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
                return;
            }
            catch (Exception err)
            {
                Trace.WriteLine($"tryHideFooter: unknown axception : {err}");
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
                Trace.WriteLine($"ResolveElementDynamic2: null obj passed");
                return;
            }
            string elem_calss = element_component.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                Trace.WriteLine($"ResolveElementDynamic2: expected one of element_component classes got {elem_calss}");
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
                OnTaskDetailChanged($"{compact_elem.companyName}/delay 500ms");
                Task.Delay(500).GetAwaiter().GetResult();
                OnTaskDetailChanged($"{compact_elem.companyName}/phone");
                compact_elem.phone = getPhone(mainWebDriver.FindElement(By.XPath(_details_section_x)));

            }
            catch  (WebDriverTimeoutException err)
            {
                Trace.WriteLine($"ResolveElementDynamic2: timed out {err.Message}");
                return;
            }
            catch (Exception err)
            {

                Trace.WriteLine($"ResolveElementDynamic2: unkown exception {err}");
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
                Trace.WriteLine("getLink: passed null object");
                return "N/A";
            }

            Debug.WriteLine($"link ..");
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                Trace.WriteLine($"getLink: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            try
            {
                return item.FindElement(By.XPath("./div[2]/a"))?.GetAttribute("href") ?? "N/A";
            }
            catch (NoSuchElementException)
            {
                Trace.WriteLine("getLink: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                Trace.WriteLine($"getLink: unknows exception: {err}");
                return "N/A";
            }
        }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">details section </param>
        /// <returns></returns>
        public static string getPhone(IWebElement item)
        {
            if (item == null)
            {
                Trace.WriteLine("getPhone: passed null object");
                return "N/A";
            }
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
                Trace.WriteLine("getPhone: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                Trace.WriteLine($"getPhone: unknown exception: {err}");
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
                Trace.WriteLine("getCategory: passed null object");
                return "N/A";
            }
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                Trace.WriteLine($"getCategory: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            Debug.WriteLine($"cat ..");
            try
            {
                return item.FindElement(By.XPath("./div[3]"))?.Text ?? "N/A";
            }
            catch (NoSuchElementException)
            {
                Trace.WriteLine("getCategory: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                Trace.WriteLine($"getCategory: unknown exception: {err}");
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
                Trace.WriteLine("getLocation: passed null object");
                return "N/A";
            }
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                Trace.WriteLine($"getLocation: expected one of element_component classes got {elem_calss}");
                return "N/A";
            }
            //the 4th div (not always,) or the class _4l12l8 (unless it's grayed out
            //here it's class _15orusq2 )
            Debug.WriteLine($"loc ..");
            try
            {
                var case_normal = item.FindElements(By.XPath("./div[@class='_4l12l8']//span[@class='_1w9o2igt']"));
                if(case_normal.Count>0) return case_normal.FirstOrDefault()?.Text??"N/A";
                else
                {
                    var case_gray = item.FindElements(By.XPath("./div[@class='_15orusq2']"));//todo add //span[@class='_1w9o2igt'] is it exists to avoid branches
                    if (case_gray.Count > 0) return case_gray.FirstOrDefault()?.Text ?? "N/A";
                    else
                    {
                        Trace.WriteLine("getLocation: counld'd find loc in 2 cases");
                        return "N/A";
                    }
                }

            }
            catch (NoSuchElementException)
            {
                Trace.WriteLine("getLocation: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                Trace.WriteLine($"getLocation: unknown exception: {err}");
                return "N/A";
            }
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
                Trace.WriteLine("getName: passed null object");
                return "N/A";
            }
            Debug.WriteLine($"name ..");
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                Trace.WriteLine($"getName: expected one of element_cmomponent classes got {elem_calss}");
                return "N/A";
            }
            try
            {
                string res = item.FindElement(By.XPath("./div[2]"))?.Text ?? "N/A";
                Debug.WriteLine($"name: {res}");
                return res;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("getName: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                Trace.WriteLine($"getName: unknown exception: {err}");
                return "N/A";
            }


        }
        /// <summary>
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
                if(pagination.GetAttribute("class")== "_1x4k6z7")
                {
                    var all_pages = pagination.FindElements(By.XPath(pages_x_));//including the selected one (whch is a div and not a)
                    pagesButtons = all_pages.Where(e => e.TagName == "a").ToArray();

                    var nav_next = pagination.FindElement(By.XPath(".//div[@class='_5ocwns']/div[2]"));
                    var nav_prev = pagination.FindElement(By.XPath(".//div[@class='_5ocwns']/div[1]"));
                    next = nav_next; prev = nav_prev;
                    isNextEnabled = next.GetAttribute("class") == enabled_nav_butt_class_;
                    curr_page_num = int.Parse(all_pages.First((e) => e.TagName == "div").Text);
                    return true;
                }
                else
                {
                    pagesButtons = null; prev = null; next = null; isNextEnabled = false;
                    curr_page_num = 0;
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Trace.WriteLine($"resolvePagination:warning: NoSuchElementException");
                pagesButtons = null; prev = null; next = null; isNextEnabled = false;
                curr_page_num = 0;
                return false;

            }
            catch (Exception err)
            {
                Trace.WriteLine($"resolvePagination:nkow axception {err}");
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
