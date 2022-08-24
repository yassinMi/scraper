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
using System.Collections.ObjectModel;

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
        /// <summary>
        /// es
        /// wats and returns false if times out or on unknow exceptions (gets reported)
        /// uses the main wabdriver
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="by"></param>
        /// <param name="tm"></param>
        public static bool WaitFor(ISearchContext sc, By by, TimeSpan tm)
        {
            WebDriverWait w = new WebDriverWait(mainWebDriver, tm);
            try
            {
                w.Until((iwd) =>
                {
                    try
                    {
                        return sc.FindElement(by) != null;
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
                        CoreUtils.WriteLine($"WaitFor:Until: unknown error: {Environment.NewLine} {err}");
                        return false;
                    }
                });
                return true;
            }
            catch (WebDriverTimeoutException err)
            {
                return false;
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"WaitFor:Until: unknown error:2: {Environment.NewLine} {err}");
                return false;
            }

        }
        /// <summary>
        /// provides sfety against exceptions in cond, returning false
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="cond"></param>
        /// <returns></returns>
        bool isFindElementSafeReturn(ISearchContext sc, Func<ISearchContext ,bool> cond)
        {
            try
            {
                return cond(sc);
            }
           
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// es
        /// exceptions are handeled so you don't need handele them in the conditin
        /// wats and returns false if times out or on unknow exceptions (gets reported)
        /// uses the main wabdriver
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="by"></param>
        /// <param name="tm"></param>
        public static bool WaitForCond(ISearchContext sc, TimeSpan tm, Func<ISearchContext,bool> cond)
        {
            WebDriverWait w = new WebDriverWait(mainWebDriver, tm);
            try
            {
                w.Until((iwd) =>
                {
                    try
                    {
                        return cond(sc);
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
                        CoreUtils.WriteLine($"WaitForCond:Until: unknown error: {Environment.NewLine} {err}");
                        return false;
                    }
                });
                return true;
            }
            catch (WebDriverTimeoutException err)
            {
                return false;
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"WaitForCond:Until: unknown error:2: {Environment.NewLine} {err}");
                return false;
            }

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











        ///[aux task]
        private void RunScraper_fromList(CancellationToken ct, string file, int getCount)
        {
            CoreUtils.WriteLine($"RunScraper_fromList.. [{DateTime.Now}], {file},{getCount}");
            lock (_lock[TargetPage])
            {

                OnResolved($"from list: {Path.GetFileName(file)}");
                ActualOutputFile =  Path.Combine(Workspace.Current.CSVOutputFolder,$"{Path.GetFileName(file)}.csv");
                int file_index = 0;
                while (File.Exists(ActualOutputFile))
                {
                    file_index++;
                    ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, $"{Path.GetFileName(file)}-{file_index}.csv");
                }
                var valid_names = File.ReadAllLines(file).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToList();
                int valid_names_count = valid_names.Count;
                if (valid_names_count == 0)
                {
                    OnStageChanged(ScrapTaskStage.Failed);
                    OnError("No valid names in the selected file");
                    return;
                }
                OnStageChanged(ScrapTaskStage.DownloadingData);
                List<Company> list = new List<Company>();
                if (Workspace.Current?.CSVOutputFolder == null) Debug.WriteLine("null ws");
                IWebElement list__; //last wrapper that ahs divs directly
                IWebElement list_wrapper_rnd; // a common wrapper for pagnation also;
                OnTaskDetailChanged("Waiting for other task(s) to end..");
                lock (_lock)//concurrent tasks can'y run this part of code simultaneously
                {

                
                //# instantate mainWebDrivr if null
                if (tryInstantiateWebDriver() == false) return;
                for (int cur_name_ix = 0; cur_name_ix < valid_names_count; cur_name_ix++)
                {
                    string cur_name = valid_names[cur_name_ix];
                    OnTaskDetailChanged($"{cur_name}/getting results..");
                    int elements_in_query = 1;
                    if (cur_name_ix == 0)
                    {
                        //#first navigation using url
                        string targetUrl = $"https://2gis.ae/search/{cur_name}";
                        Debug.WriteLine("nav");
                        try
                        {
                            mainWebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                            CoreUtils.WriteLine("SwitchTo() ..");
                            mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                            OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                            mainWebDriver.Url = targetUrl;
                            CoreUtils.WriteLine($"Navigate()..[{DateTime.Now}] '{targetUrl}'");
                            mainWebDriver.Navigate();
                        }
                        catch (Exception err)
                        {
                            CoreUtils.WriteLine($"couldnt navigate :unknown error {err}");
                            OnError(err.Message);
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                    }
                    else
                    {
                        //#ubsequent navigations using form input submision

                        //#locate form_input
                        IWebElement form_input = null;
                        try
                        {
                            var pre_form_input = mainWebDriver.FindElements(By.XPath(".//div[@class='_ubuirc']//form//input"));
                            if (pre_form_input.Any() == false)
                            {
                                CoreUtils.WriteLine($"no elements found");
                            }
                            var pre = pre_form_input
                               .Where(i => (i.GetAttribute("class") == "_1dhzhec9" )||( i.GetAttribute("class") == "_1gvu1zk"));//two classes one for the norma state the second when input is being edited
                            if (pre.Any() == false)
                            {
                                CoreUtils.WriteLine($"expected classes not found, got {string.Join(", ", pre_form_input.Select(i=>i.GetAttribute("class")))}");
                            }
                            form_input = pre.FirstOrDefault();
                        }
                        catch (Exception err)
                        {
                            CoreUtils.WriteLine($"cannot find form_nput: {err}");
                            OnError("cannot find form_nput unkown error");
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                        if (form_input == null)
                        {
                            CoreUtils.WriteLine($"cannot find form_nput");
                            OnError("cannot find form_nput");
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                        //# perform query submtion
                        try
                        {
                            form_input.Clear();
                            form_input.SendKeys(Keys.Control+"a");
                            form_input.SendKeys(Keys.Backspace);
                            form_input.SendKeys("\b");
                            form_input.SendKeys(Keys.Backspace);


                            form_input.SendKeys(cur_name);
                            form_input.Submit();
                        }
                        catch (Exception err)
                        {
                            CoreUtils.WriteLine($"perform query submition failed:{Environment.NewLine} {err}");
                            OnError("perform query submition failed");
                            OnStageChanged(ScrapTaskStage.Failed);
                            return;
                        }
                        

                    }

                    //#wait for visible search results 
                    //# wait for elements visibility
                    OnTaskDetailChanged("waiting for elemetns content");
                    WebDriverWait w = new WebDriverWait(mainWebDriver, TimeSpan.FromSeconds(30));
                    try
                    {
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
                    }
                    catch (Exception err)
                    {
                        CoreUtils.WriteLine($"waiting failed [{DateTime.Now}]:{Environment.NewLine}{err}");
                        OnError(err.Message);
                        OnStageChanged(ScrapTaskStage.Failed);
                        return;
                    }


                    ReadOnlyCollection<IWebElement> elements_divs = null;
                    bool emptyResultsOrSomethingWentWrong = false;
                    //# get the elements_divs and emptyResults flag values; 
                    OnTaskDetailChanged("locating elements_divs");
                    list_wrapper_rnd = mainWebDriver.FindElement(
                                By.XPath(".//div[@class='_z72pvu']//div[@class='_1667t0u']"));
                    list__ = list_wrapper_rnd.FindElement(By.XPath(".//div[@class='_awwm2v']"));
                    elements_divs = list__.FindElements(By.XPath($"./div[not(@class='_106bqvr')]"));
                    emptyResultsOrSomethingWentWrong = elements_divs == null || elements_divs.Any() == false;


                    //#hide footer (important as phone expanding button gets hiiden by it)
                    OnTaskDetailChanged("hide footer..");
                    tryHideFooter();
                    //#processing
                    OnTaskDetailChanged("processing..");

                    if (emptyResultsOrSomethingWentWrong)
                    {
                        continue;
                    }

                    int first_page_elements_count = elements_divs.Count;
                    int max_results_count = Math.Min(getCount, first_page_elements_count);
                    for (int sr_ix = 0; sr_ix < max_results_count; sr_ix++)
                    {
                        var div = elements_divs[sr_ix];
                        var act = new OpenQA.Selenium.Interactions.Actions(mainWebDriver);
                        try
                        {
                            act.ScrollToElement(elements_divs[Math.Min(sr_ix + 1, elements_divs.Count - 1)]);
                            act.Perform();
                        }
                        catch (Exception err)
                        {
                            CoreUtils.WriteLine($"act.Perform failed: {err}");
                            Trace.Fail("act.Perform failed", err.ToString());
                        }
                        Task.Delay(80).GetAwaiter().GetResult();
                        Company new_cmp_elem = new Company();
                        var element_component = div.FindElement(By.XPath("./div"));
                        new_cmp_elem.companyName = getName(element_component);
                        OnTaskDetailChanged($"{cur_name}/{sr_ix}:{new_cmp_elem.companyName}/location");
                        new_cmp_elem.location = getLocation(element_component);
                        OnTaskDetailChanged($"{cur_name}/{sr_ix}:{new_cmp_elem.companyName}/branches");
                        new_cmp_elem.branches = getBranchesNum(element_component);
                        OnTaskDetailChanged($"{cur_name}/{sr_ix}:{new_cmp_elem.companyName}/category");
                        new_cmp_elem.category = getCategory(element_component);
                        OnTaskDetailChanged($"{cur_name}/{sr_ix}:{new_cmp_elem.companyName}/link");
                        new_cmp_elem.link = getLink(element_component);
                        ResolveElementDynamic2(new_cmp_elem, element_component);
                        Debug.WriteLine($"delaying ..");
                        Task.Delay(20).GetAwaiter().GetResult();
                        list.Add(new_cmp_elem);
                        TaskStatsInfo.incElem(1);
                        if (ct.IsCancellationRequested)
                        {
                            if (IsStopRequested == false)
                            {
                                OnIsStopRequestedChanged(true);
                                CoreUtils.WriteLine($"canceled at element:'{sr_ix}', search quqery index:'{cur_name_ix}'");
                            }
                            break;
                        }
                    }
                    CSVUtils.CSVWriteRecords(ActualOutputFile, list, false);

                    TaskStatsInfo.incElem(elements_in_query);
                    OnProgress(new DownloadingProg() { Total = valid_names_count, Current = cur_name_ix + 1 });

                }
                }


                OnStageChanged(ScrapTaskStage.Success);
            }

            return;
        }


















        /// <summary>
        /// es
        /// if(!tryInstantiateWebDriver()) return;
        /// usage within task object
        /// all error reportn and stage managment is handeled
        /// </summary>
        /// <example> <code>if(!tryInstantiateWebDriver()) return;</code></example>
        /// <returns></returns>
        bool tryInstantiateWebDriver()
        {
            if (mainWebDriver == null)
            {
                OnStageChanged(ScrapTaskStage.Setup);
                OnTaskDetailChanged("Starting chrome..");
                CoreUtils.WriteLine("Starting driver..");
                try
                {
                    mainWebDriver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), new ChromeOptions(), TimeSpan.FromMinutes(3));
                    return true;
                }
                catch (DriverServiceNotFoundException err)
                {
                    CoreUtils.WriteLine($"couldn't start chrome: {err}");
                    OnError(err.Message);
                    OnStageChanged(ScrapTaskStage.Failed);
                    return false;
                }
                catch (Exception err)
                {
                    CoreUtils.WriteLine($"couldn't start chrome:unknown error {err}");
                    OnError(err.Message);
                    OnStageChanged(ScrapTaskStage.Failed);
                    return false;
                }
            }
            return true; //todo check wd state and fail or proceed accordingly
        }







        public override async Task RunScraper(CancellationToken ct)
        {
            //# aux task forwarding (temporary todo refactor)
            bool isAuxiliaryTask = false;
            string aux_header;
            string[] aux_params;
            isAuxiliaryTask = CoreUtils.TryParseAuxiliaryTaskQuery(TargetPage, out aux_header, out aux_params);
            if (isAuxiliaryTask) {
                if (aux_header == "fromList")
                {
                    int maxCount = 1;
                    int.TryParse(aux_params[1], out maxCount);
                    RunScraper_fromList(ct,aux_params[0], maxCount);
                    return;
                }
                else
                {
                    OnStageChanged(ScrapTaskStage.Failed);
                    OnError($"task type '{aux_header}' not supported");
                    return;
                }
            };
            //# normal task start
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
                    //#instantate wd
                    if (!tryInstantiateWebDriver()) return;
                    Debug.WriteLine("url driver");
                    OnStageChanged(ScrapTaskStage.DownloadingData);
                    Debug.WriteLine("nav");
                    try
                    {
                        mainWebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                        CoreUtils.WriteLine("SwitchTo() ..");
                        mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                        OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                        mainWebDriver.Url = TargetPage;
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
                    int desired_initial_page = current_page;
                    bool first_take = true;//n th next do loop
                    do
                    {
                        try
                        {
                            
                            
                            //# wait for elements visibility
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
                                if (desired_initial_page != 1)
                                {
                                    ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, CoreUtils.SanitizeFileName(mainWebDriver.Title) +$"-from-page-{desired_initial_page}"+ ".csv");

                                }
                                else
                                {
                                    ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, CoreUtils.SanitizeFileName(mainWebDriver.Title) + ".csv");

                                }

                                if (File.Exists(ActualOutputFile))
                                {
                                    Trace.Fail($"Starting this task will replace an existing scv file '{ActualOutputFile}' {Environment.NewLine} if you don't want to lose the old file rename it or make a copy of it, then click 'ignore' to continue");
                                }
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
                        //#skipnig to the required page
                        if (first_take && desired_initial_page != 1)
                        {
                            OnTaskDetailChanged("skipnig to the required page number");
                            skipToPage(list_wrapper_rnd, desired_initial_page);
                            OnTaskDetailChanged("waiting for elemetns_content");
                            WebDriverWait w = new WebDriverWait(mainWebDriver, TimeSpan.FromSeconds(130));
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
                            Debug.WriteLine("getting categories_welements_wrapper..");
                            //# listing
                            //elems_content is _z72pvu ([2nd]after filr) o>  _3zzdxk o> _1667t0u o> _1rkbbi0x o> _15gu4wr 
                            //list is: div[2rd] no class o> _awwm2v
                            //currentlyusing .//div[@class='_z72pvu']//div[@class='_1667t0u']
                            list_wrapper_rnd = mainWebDriver.FindElement(
                                By.XPath(".//div[@class='_z72pvu']//div[@class='_1667t0u']"));
                            list__ = list_wrapper_rnd.FindElement(By.XPath(".//div[@class='_awwm2v']"));

                        }

                        // By.XPath("/*[@class='Sidebar__Container-gs0c67-0 bXQeSB sidebar']"));
                        Debug.WriteLine($"enumerating categories_welements.. is null {list__.GetDomProperty("innerHTML")}");
                        var elements_divs = list__.FindElements(By.XPath($"./div[not(@class='_106bqvr')]"));//_106bqvr is advertisement class
                        Debug.WriteLine($"elements_divs null {elements_divs == null} ");
                        Debug.WriteLine($"elements_divs count {elements_divs.Count} ");


                        int i = 0;
                        CoreUtils.WriteLine($"page started [{DateTime.Now}]:{current_page}");
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
                            OnTaskDetailChanged($"{new_cmp_elem.companyName}/branches");
                            new_cmp_elem.branches = getBranchesNum(element_component);
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
                        first_take = false;
                    } while (should_scrape_next_page);

                    
                }
                    

                OnStageChanged(ScrapTaskStage.Success);
                return;

            }
        }

        private void skipToPage(IWebElement elems_conts, int desired_initial_page)
        {
            Debug.WriteLine($"skipToPage: {desired_initial_page}");
            IWebElement next = null, prev;
            IWebElement[] pages_butts = null;
            bool isNextEnabled = false, exists_next_page = false;
            int curr_page_num = 0;
            exists_next_page = resolvePagination(elems_conts, out pages_butts, out next, out prev, out isNextEnabled, out curr_page_num) && isNextEnabled;
            bool atDesiredPage = curr_page_num == desired_initial_page;
            while (!atDesiredPage)
            {
                CoreUtils.WriteLine($"it:  curr_page_num:'{curr_page_num}',isNextEnabled:'{isNextEnabled}',pages_buttons:'{ (pages_butts == null ? "null" : string.Join(",", pages_butts.Select(b => b.Text)))}'");

                var maybe_dpb = pages_butts.FirstOrDefault(p => getElementText(p) == $"{desired_initial_page}");
                if (maybe_dpb != null)
                {
                    maybe_dpb.Click();
                    return;
                }
                else 
                {
                    IWebElement highest_visible = pages_butts
                        .OrderByDescending((p) =>int.Parse(getElementText(p))).First();
                    Debug.WriteLine($"highest page button {getElementText(highest_visible)}");
                    highest_visible.Click();
                }
                exists_next_page = resolvePagination(elems_conts, out pages_butts, out next, out prev, out isNextEnabled, out curr_page_num) && isNextEnabled;
                atDesiredPage = curr_page_num == desired_initial_page;

            }
            return;
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
            if (element_component == null) { CoreUtils.WriteLine($"ResolveElementDynamic2: null obj passed"); return; }
            string elem_calss = element_component.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS || elem_calss == ELEMENT_CLASS_SELECTED || elem_calss == ELEMENT_CLASS_HOVERED || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"ResolveElementDynamic2: expected one of element_component classes got {elem_calss}");
                return;//todo should return only in a strict_dev_mode, otherwise keep going as long as exception safe
            }
            try
            {
                OnTaskDetailChanged($"{compact_elem.companyName}/waiting for company details");
                element_component.Click();
                if (!WaitFor(mainWebDriver, By.XPath(_details_section_x ), TimeSpan.FromSeconds(25)))
                {
                    CoreUtils.WriteLine($"ResolveElementDynamic: timed out after 25 seconds, name: '{compact_elem.companyName}', link:'{compact_elem.link}, page: '{Page}'");
                    return;
                }
                var details_sect = mainWebDriver.FindElement(By.XPath(_details_section_x));

                
                //this waits for at least one of: phone dd, email dd, website dd, the P.O.Box 3945 dd
                var phone_indicator =  By.XPath(".//div[@class='_49kxlr']/div[@class='_b0ke8']/a[@class='_2lcm958']");
                var website_indicator = By.XPath(".//div[@class='_172gbf8']//div[@class='_49kxlr']/span/div/a[@class='_1rehek']");
                var dataDividers_locator = By.XPath(".//div[@class='_599hh'and@data-rack]/div");
                ////div[@class='_599hh'and@data-rack]/div the dd's shoul be more than 2

                if (!WaitForCond(mainWebDriver, TimeSpan.FromSeconds(35),sc=> {

                    //force false if only 2 or less data dividers
                    if (sc.FindElements(dataDividers_locator).Count <= 2) return false;
                    //force true if phone exists or website
                    if (isFindElementSafeReturn(sc, s => s.FindElement(phone_indicator) != null)) return true ;
                    if (isFindElementSafeReturn(sc, s => s.FindElement(website_indicator) != null)) return true;
                    
                    return false;
                }))
                {
                    CoreUtils.WriteLine($"ResolveElementDynamic: waiing phone or website.. timed out after 35 seconds, name: '{compact_elem.companyName}', link:'{compact_elem.link}, page: '{Page}'");
                    return;
                }
                OnTaskDetailChanged($"{compact_elem.companyName}/delay 10ms");
                Task.Delay(10).GetAwaiter().GetResult();
                OnTaskDetailChanged($"{compact_elem.companyName}/phone");
                compact_elem.phone = getPhone(details_sect);
                compact_elem.email = getEmail(details_sect);
                compact_elem.website = getWebste(details_sect);
                compact_elem.category = getCatogory2(details_sect, compact_elem.companyName);//todo fllback to the old category approach

            }
            catch (Exception err)
            {

                CoreUtils.WriteLine($"ResolveElementDynamic: (name: '{compact_elem.companyName}', link:'{compact_elem.link}, page: '{Page}'), unkown exception {err}");
                return;
            }
        }
        /// <summary>
        /// es
        /// returns false when something is wrong, (subsequent parsing should be aborted
        /// </summary>
        /// <returns></returns>
        bool switchToInfoTab(IWebElement details_section)
        {
            //#locate info tab item 
            IWebElement info_tab_item = null;
            try
            {
                var ino_tab_item_locator_x = "//div[@class='_1kmhi0c'][.//a[text()='Info']]";//from details_section
                info_tab_item = details_section.FindElement(By.XPath(ino_tab_item_locator_x));

            }
            catch (Exception err)
            {
                if (err is NoSuchElementException)
                    CoreUtils.WriteLine($"switchToInfoTab: NoSuchElementException ");
                else
                {
                    CoreUtils.WriteLine($"switchToInfoTab: unknown: {Environment.NewLine}{err} ");
                }
                return false;
            }
            if (info_tab_item == null) return false;
            try
            {
                info_tab_item.Click();
                return true;
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"switchToInfoTab: unkclick failed: {Environment.NewLine}{err} ");
                return false;
            }
            //#click
        }
        /// <summary>
        /// gets the correct category(s), this also switches to the info tab, make sure to call only fter done with the contacts tab
        /// </summary>
        /// <param name="details_section"></param>
        /// <returns></returns>
        string getCatogory2(IWebElement details_section, string elemName=null)
        {
            //#switch to info tab
            if (!switchToInfoTab(details_section))
            {
                CoreUtils.WriteLine($"getCatogory2: filed to switch tab (elem_name='{elemName}')");
                return "N/A";
            }
            //# wait for required data to be visible
            string cat_data_rank_shortcut_locator_x = "//div[@class='_18lzknl']//div[@class='_599hh'][@data-rack='true'][./div[@class='_172gbf8'][1]/div[@class='_49kxlr']/span[@class='_btwk9a2'][text()='Categories']]";
            //the above takes straight to cat data rank, making sure it exists as a way to avoid bugs in the subsequent parsing
            if(!WaitFor(mainWebDriver, By.XPath(cat_data_rank_shortcut_locator_x), TimeSpan.FromSeconds(8)))
            {
                CoreUtils.WriteLine($"getCatogory2: [{DateTime.Now}] timed out after 8 seconds");
                return "N/A";
            };

            var dr = details_section.FindElement(By.XPath(cat_data_rank_shortcut_locator_x));
            
            //#locate categories in single case
            string cat_single_case_locator_x_ = "./div[2]//span[@class='_oqoid']";
            //the above from dr and return the span with direct cat name
            try
            {
                var cat = dr.FindElement(By.XPath(cat_single_case_locator_x_));
                return getElementText(cat);
            }
            catch (NoSuchElementException)
            {
                //pass to multiple
            }

            //locate categories in multple case
            string cat_mult_case_locator_x_ = "./div[2]//span[@class='_14quei']";
            //the above from dr and return the direct wraper of spans
            try
            {
                var cat = dr.FindElement(By.XPath(cat_mult_case_locator_x_));
                var cats = cat.FindElements(By.XPath(".//a[@class='_1rehek']"));
                return string.Join(" • ", cats.Select(c => getElementText(c)));
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine($"getCatogory2: NoSuchElementException: {dr.GetAttribute("innerHTML")}");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getCatogory2: unknow:{Environment.NewLine} {err}");
                return "N/A";
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
                return string.Join(" / ", ee.Select(a => a?.GetAttribute("href").Replace("tel:","")));
                
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
        /// <param name="item">details section </param>
        /// <returns></returns>
        public string getEmail(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getEmail: passed null object");
                return "N/A";
            }
            //matches phone and othern a test is needed
            //.//div[@class='_172gbf8']//div[@class='_49kxlr']/div/a[@class='_2lcm958']
            Debug.WriteLine($"email ..");
            try
            {
                var fe = item.FindElements(By.XPath(".//div[@class='_172gbf8']//div[@class='_49kxlr']/div/a[@class='_2lcm958']"));///
                if (fe == null) return "N/A";
                if (fe.Any() == false) return "N/A";
                var email_one = fe.FirstOrDefault(a =>
                {
                    string href = a.GetAttribute("href");
                    if (string.IsNullOrWhiteSpace(href)) return false;
                    return href.Trim().StartsWith("mailto:");

                });
                if(email_one==null) return "N/A";
                return email_one.GetAttribute("href").Trim().Replace("mailto:", "");
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getEmail: NoSuchElementException");
                return "N/A";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getEmail: unknown exception: {err}");
                return "N/A";
            }
        }


        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">details section </param>
        /// <returns></returns>
        public string getWebste(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getWebste: passed null object");
                return "N/A";
            }

            //.//div[@class='_172gbf8']//div[@class='_49kxlr']/span/div/a[@class='_1rehek']
            Debug.WriteLine($"website ..");
            try
            {
                var fe = item.FindElements(By.XPath(".//div[@class='_172gbf8']//div[@class='_49kxlr']/span/div/a[@class='_1rehek']"));///
                if (fe == null) return "";
                if (fe.Any() == false) return "";
                foreach (var a in fe)
                {
                    string inner = getElementText(a);
                    if (string.IsNullOrWhiteSpace(inner)) continue;
                    Uri u;
                    if (Uri.TryCreate(inner, UriKind.Absolute, out u))  continue;
                    return inner;
                }

                return "";
            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getWebste: NoSuchElementException");
                return "";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getWebste: unknown exception: {err}");
                return "";
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
                    return s?.Trim();
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
                        return s?.Trim();
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

        /// <summary>
        /// es
        /// </summary>
        /// <param name="item">element_component</param>
        /// <returns></returns>
        private string getBranchesNum(IWebElement item)
        {
            if (item == null)
            {
                CoreUtils.WriteLine("getBranchesNum: passed null object");
                return "";
            }
            string elem_calss = item.GetAttribute("class");
            if (!(elem_calss == ELEMENT_CLASS
             || elem_calss == ELEMENT_CLASS_SELECTED
             || elem_calss == ELEMENT_CLASS_HOVERED
             || elem_calss == ELEMENT_CLASS_HOVERED_SELECTED))
            {
                CoreUtils.WriteLine($"getBranchesNum: expected one of element_component classes got {elem_calss}");
                return "";
            }
            //the 4th div (not always,) or the class _4l12l8 (unless it's grayed out
            //here it's class _15orusq2 )
            Debug.WriteLine($"BranchesNum ..");
            try
            {
                var case_normal = item.FindElements(By.XPath("./div[@class='_4l12l8']//span[@class='_1w9o2igt']"));

                if (case_normal.Count > 0)
                {
                    foreach (var a in case_normal)
                    {
                        string s = getElementText(a);
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            continue;
                        }
                        if (s.Trim().Contains("branches"))
                        {
                            return s.Trim().Replace("branches", "").Trim();
                        }
                    }
                    return "";
                }
                else
                {
                    var case_gray = item.FindElements(By.XPath("./div[@class='_15orusq2']"));//todo add //span[@class='_1w9o2igt'] is it exists to avoid branches
                    if (case_gray.Count > 0)
                    {
                        foreach (var a in case_gray)
                        {
                            string s = getElementText(a);
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                continue;
                            }
                            if (s.Trim().Contains("branches"))
                            {
                                return s.Trim().Replace("branches", "").Trim();
                            }
                        }
                        return "";
                    }

                    else
                    {
                        return "";
                    }
                }

            }
            catch (NoSuchElementException)
            {
                CoreUtils.WriteLine("getBranchesNum: NoSuchElementException");
                return "";
            }
            catch (Exception err)
            {
                CoreUtils.WriteLine($"getBranchesNum: unknown exception: {err}");
                return "";
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
