using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using BayutPlugin.Model;
using scraper.Core.UI;
using System.Threading;
//using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

namespace BayutPlugin
{
    public class BayutScrapingTask: ScrapingTaskBase
    {
        public BayutScrapingTask(string tp)
        {
            TargetPage = tp;
        }

        public static string downloadOrRead(string pageLink, string folder, bool force_update = false)
        {
            Debug.WriteLine($"downloadOrRead '{pageLink}'");
            string uniqueFilename = Path.Combine(folder, CoreUtils.getUniqueLinkHash(pageLink) + ".html");
            if (force_update == false && File.Exists(uniqueFilename)) { return File.ReadAllText(uniqueFilename); }
            else
            {
                Task.Delay(4).GetAwaiter().GetResult();//todo make specified as arg
                int retry_count = 0;
                retry:
                retry_count++;
                try
                {
                    string rawElementPage = WebHelper.instance.GetPageTextSync(pageLink);
                    File.WriteAllText(uniqueFilename, rawElementPage);
                    return rawElementPage;
                }
                catch (Exception err)
                {
                    Debug.WriteLine($"downloadOrRead throw {err.Message}");
                    //string u_response = "CANCEl";
                    /*CoreUtils.RequestPrompt(new PromptContent($"Couldn't fetch resource '' because of a network error{Environment.NewLine}press Ok to retry{Environment.NewLine}Press Cancel to abort task", "error", new string[] { "OK", "CANCEL" }, PromptType.Error),
                        (response) => {
                            Debug.WriteLine("resp:" + response);
                            u_response = response;
                        });
                    if (u_response == "OK") goto retry;*/
                    if (retry_count > 5) throw;
                    else goto retry;
                }
            }
        }

        /// <summary>
        /// handle all fatal error by calling this, it will dump file, and warn the user and set task status.
        /// </summary>
        /// <param name="dump"></param>
        /// <param name="silent">not saving dump and not showing message box,</param>
        private void abortTask(string hint, List<BProperty> dump, int start_page, int end_page, bool silent = false)
        {
            if (dump.Count > 0)
            {
                int p_from = start_page;
                int p_to = end_page;
                string saved_pages_range_str = (p_to == p_from ? $"page {p_from}" : $"pages {p_from}-{p_to}");
                string dump_file = ActualOutputFile.Substring(0, ActualOutputFile.Length - 4);//removing extesntion .csv
                dump_file = dump_file + " (" + saved_pages_range_str + ").csv";
                if (silent == false)
                    CSVUtils.CSVWriteRecords(Path.GetFileName(dump_file), dump, false);
                CoreUtils.WriteLine($"aborted: [{DateTime.Now}], {saved_pages_range_str}, count: {dump.Count} ");
                if (silent == false)
                    CoreUtils.RequestPrompt(new PromptContent($"{hint}{Environment.NewLine}{dump.Count} properties records at {saved_pages_range_str} are saved here:{dump_file}", "Task aborted", new string[] { "OK" }, PromptType.Error), s => { });

            }

            else
            {
                //# nothing to save
                CoreUtils.WriteLine($"aborted: [{DateTime.Now}], with no records");
                if (silent == false)
                    CoreUtils.RequestPrompt(new PromptContent($"{hint}{Environment.NewLine}{"no properties records were collected."}", "Task aborted", new string[] { "OK" }, PromptType.Error), s => { });
            }
            CoreUtils.WriteLine($"[REPORT][{DateTime.Now}]");
            CoreUtils.WriteLine($"t lvl report: {Environment.NewLine}{ReportBuilderTaskLevel.ToString()}");
            CoreUtils.WriteLine($"p lvl report: {Environment.NewLine}{ReportBuilderPageLevel.ToString()}");
            CoreUtils.WriteLine($"e lvl report: {Environment.NewLine}{ReportBuilderElementLevel.ToString()}");
            CoreUtils.WriteLine("[ENDOF REPORT]");
            OnTaskDetailChanged(null);
            OnError(hint);
            OnStageChanged(ScrapTaskStage.Failed);


        }



        private bool isMultiplePagesBPropertysListings(JObject state)
        {
            return true;//todo
        }

        private bool isNoneEmptyBPropertysListings(JObject state)
        {
            return true;
        }

        private bool isBPropertysListings(JObject state)
        {
            return true;
        }

        public string GetElementUniqueID(string elementRootNode)
        {
            return "lk";
        }

        public string GetElementUserID(string elementRootNode)
        {
            return "k";
        }

        public string GetElementUserUniqueID(string elementRootNode)
        {
            return "oj";
        }

        public string GetPageUniqueUserTitle(HtmlNode pageNode)
        {
            // //div[@id='search-header']//h1
            var h1 = pageNode.SelectSingleNode(header_locator_x);
            return h1.InnerText;
        }

        const string header_locator_x = "//div[@id='search-header']//h1";

        public bool HasElementsTBF(HtmlNode pageNode)
        {
            return true;
        }







        private string ClearPageNumFromUrl(string targetRootPage)
        {
            Debug.WriteLine($"ClearPageNumFromUrl : '{targetRootPage}");
            //https://www.bayut.com/to-rent/property/dubai/page-5
            //https://www.bayut.com/to-rent/offices/dubai/page-2/?furnishing_status=furnished
            return Regex.Replace(targetRootPage, @"/page-\d+", "");

        }

        private string AppendPageNumToUrl(string p, int pg)
        {
            if (pg == 1) return p;//bayut doesnt allow page-1 returning 404
            Debug.WriteLine($"AppendPageNumToUrl : '{p}");
            string res;
            Uri u;
            Uri.TryCreate(p, UriKind.Absolute, out u);
            if (string.IsNullOrWhiteSpace(u.Query))
            {
                res = u.OriginalString.TrimEnd('/') + $"/page-{pg}";
            }
            else
            {
                res = u.AbsolutePath.TrimEnd('/') + $"/page-{pg}/"+"?"+u.Query;
            }

            Debug.WriteLine($"res : '{res}");
            return res;

        }

        /// <summary>
        /// throws exception "coulndn't extract nbpage information"
        /// </summary>
        /// <param name="stateObject"></param>
        /// <returns></returns>
        private int getLastPageNumberFromJson(JObject stateObject)
        {
            string as_str= stateObject.SelectToken("$.algolia.content.nbPages").ToString();
            if (string.IsNullOrWhiteSpace(as_str)) throw new Exception("coulndn't extract nbpage information, missing");
            int nb = 0;
            if(!int.TryParse(as_str,out nb)) { throw new Exception("coulndn't extract nbpage information, not a number"); }
            return nb;
        }



        /// <summary>
        /// returns the full state object
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        string getJsonBpropertysListingPayload(string doc)
        {
            //between  window.propertyfinder.settings.agent =  .. window.propertyfinder.settings.gtm 
            string d1 = "window.state = ";
            string d2 = "window.webpackBundles =";
            var first_ix = doc.IndexOf(d1);
            var last_ix = doc.IndexOf(d2);
            string res = doc.Substring(first_ix + d1.Length, last_ix - (first_ix + d1.Length))
                .Trim().TrimEnd(';');
            Debug.WriteLine($"yass{res}yass");
            return res;
        }


        /// <summary>
        /// can throw exception when user don't press retry
        /// 
        /// int start, int end, int current, state object , string raw page
        /// </summary>
        /// <param name="rootPageUrl"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<int, int,int, JObject, string>> EnumeratePages(string startingPageUrl, CancellationToken ct)
        {
            string raw_first;
            bool should_ask_skip_page = true;
            int desiredStartingPage = 1;
            int startingUrlPageNum = getPageNumberFromUrl(startingPageUrl);
            Debug.WriteLine($"staring page:{startingUrlPageNum}");
            if (startingUrlPageNum != 1)
            {
                //# propmt for starting page
                Func<int, string> format_page_range_str = (int start_p) => {
                    int length = start_p - 1;
                    return $"{(length > 1 ? "pages" : "page")} {(length > 1 ? $"(1-{start_p - 1})" : "1")}";
                };
                string p_s_resp = "";
                CoreUtils.RequestPrompt(new PromptContent($"Do you want to skip elements from {format_page_range_str(startingUrlPageNum)} and start from page {startingUrlPageNum}?"
                    , $"Start from page {startingUrlPageNum}?"
                    , new string[] { "Yes", "Start from page 1" }, PromptType.Question),
                    (response) => {p_s_resp = response;});
                if (p_s_resp.ToLower() == "yes") desiredStartingPage = startingUrlPageNum;
                else desiredStartingPage = 1;
            }
            retry:
            try
            {
                raw_first = downloadOrRead(startingPageUrl, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
            }
            catch (Exception err)
            {
                string u_response = "CANCEl";
                CoreUtils.WriteLine($"EnumeratePages: [{DateTime.Now}] {startingPageUrl}, {err}");
                CoreUtils.RequestPrompt(new PromptContent($"Page: {startingPageUrl}{Environment.NewLine}{Environment.NewLine}Error details: '{err.Message}"
                    , $"Could not fetch the first page!"
                    , new string[] { "Retry", "Cancel task" }, PromptType.Error),
                    (response) => {
                        Debug.WriteLine("resp:" + response);
                        u_response = response;
                    });
                if (u_response.ToLower() == "retry") goto retry;
                else if (u_response.ToLower() == "cancel task") throw;
                else throw;
            }


            int min = 1;
            int max = 1;
            JObject full = JObject.Parse(getJsonBpropertysListingPayload(raw_first));
            if (isMultiplePagesBPropertysListings(full))
            {
                max = getLastPageNumberFromJson(full);
                CoreUtils.WriteLine($"max page:" + max);
            }
            min = Math.Min(desiredStartingPage, max);//todo warn about no pages to scrap and break
            foreach (var pg in Enumerable.Range(min, max))
            {
                OnTaskDetailChanged($"Downloading page {pg}..");
#if SKIP_PAGES_IN_THE_MIDDLE
                if (pg < 430 && pg > 14) continue;
#endif
                string pageLink = AppendPageNumToUrl(ClearPageNumFromUrl(startingPageUrl), pg);
                string raw_current;
                JObject full_current;
                retry_sebsequent:
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }
                try
                {
#if LOOP_IN_FIRST_PAGE //looping at the first page only (where elements are pre dowloaded in the test ws
                    raw = downloadOrRead(rootPageUrl, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
#else
                    raw_current = downloadOrRead(pageLink, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
                    full_current = JObject.Parse(getJsonBpropertysListingPayload(raw_current));
#endif

                }
                catch (Exception err)
                {
                    string u_response = "skip page"; //default
                    CoreUtils.WriteLine($"EnumeratePages: [{DateTime.Now}] {pageLink}, {err}");
                    if (should_ask_skip_page)
                        CoreUtils.RequestPrompt(new PromptContent($"Page url: {pageLink}{Environment.NewLine}{Environment.NewLine}(if you stop now {global_couner} of {total_count} properties will be saved){Environment.NewLine}Error details: '{err.Message}"
                    , $"Could not fetch page ({pg})!"
                    , new string[] { "Retry", "Skip", "Skip all", "Stop task" }, PromptType.Error),
                    (response) => {
                        Debug.WriteLine("resp:" + response);
                        u_response = response;
                    });
                    if (u_response.ToLower() == "retry") goto retry_sebsequent;
                    else if (u_response.ToLower() == "skip") continue;
                    else if (u_response.ToLower() == "skip all") { should_ask_skip_page = false; continue; }
                    else if (u_response.ToLower() == "stop task") throw;
                    else continue;
                }
                TaskStatsInfo.incSize(raw_current.Length);
                HtmlDocument newDoc = new HtmlDocument();
                newDoc.LoadHtml(raw_current);
                yield return new Tuple<int, int,int, JObject, string>(min, max, pg, full_current, raw_current);
            }

        }
        /// <summary>
        /// extracts page number from url for bayut
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        private int getPageNumberFromUrl(string Url)
        {
            //https://www.bayut.com/to-rent/property/dubai/page-5
            //https://www.bayut.com/to-rent/offices/dubai/page-2/?furnishing_status=furnished
            var m = Regex.Match(Url, @"/page-(\d+)");
            if (!m.Success) return 1;
            int p;
            if (!int.TryParse(m.Groups[1].Value, out p)) return 1;
            return p;
        }





        /// <summary>
        /// tuple: prop compact, (not  needed)
        /// </summary>
        /// <param name="full">the state object of the listing page</param>
        /// <returns></returns>
        public IEnumerable<Tuple<BProperty, string,JToken>> EnumerateCompactElements(JObject full)
        {
            //var obj = JsonConvert.DeserializeObject<dynamic>(json);
            var j = full;
            Debug.WriteLine($"j is null is {j==null}");
            Debug.WriteLine($"j is  {j .ToString()}");
            var content = j.SelectToken("$.algolia.content");
            if (content == null)
            {
                throw new Exception("couldn't start parsing page, contents null");
            }

            if (total_count == 0)
            {
                int.TryParse(content.SelectToken("$.nbHits").ToString(), out total_count);
                Debug.WriteLine($"total count is {total_count}");
            }
            Debug.WriteLine("content located");
            var elems = content.SelectTokens("$.hits[*]");
            Debug.WriteLine(elems.Count());
            foreach (var prop in elems)
            {
                Debug.WriteLine("prop found");
                BProperty p = new BProperty();
                //early title parsing or ui feedback mechanics.
                p.Title = prop.SelectToken("$.title").ToString();
                //id is not reuired (incase need in the future)
                string id = prop.SelectToken("$.id").ToString();
                Debug.WriteLine("prop return");
                yield return new Tuple<BProperty, string,JToken>(p,id, prop);
            }
        }


        void ResolveElement(JToken prop, BProperty p , out int obj_cc, out long bytes )
        {
            p.agencyName = prop.SelectToken("$.agency.name")?.ToString()??"";
            p.area = transformAreaToftsq(prop.SelectToken("area")?.ToString());
            p.baths = prop.SelectToken("$.baths")?.ToString() ?? "N/A";
            p.category = stringifyCategory(prop.SelectToken("$.category"));
            p.completionStatus = prop.SelectToken("$.completionStatus")?.ToString() ?? "N/A";
            p.contactName = prop.SelectToken("$.contactName")?.ToString();
            p.createdAt = transformDateTime(prop.SelectToken("$.createdAt")?.ToString());
            p.furnishingStatus = prop.SelectToken("$.furnishingStatus")?.ToString()??"N/A";
            p.hasMatchingFloorPlans = prop.SelectToken("$.hasMatchingFloorPlans")?.ToString();
            p.location = stringifyLocation(prop.SelectToken("$.location"));
            p.mobile = prop.SelectToken("$.phoneNumber.mobile")?.ToString();
            p.phone = prop.SelectToken("$.phoneNumber.phone")?.ToString();
            p.price = prop.SelectToken("$.price")?.ToString() ?? "N/A";
            p.proxyMobile = prop.SelectToken("$.phoneNumber.proxyMobile")?.ToString()??"";
            p.purpose = prop.SelectToken("$.purpose")?.ToString() ?? "N/A";
            p.referenceNumber = prop.SelectToken("$.referenceNumber")?.ToString()??"N/A";
            p.rentFrequency = prop.SelectToken("$.rentFrequency")?.ToString()??"N/A";
            p.rooms = prop.SelectToken("$.rooms")?.ToString()??"N/A";
            p.updatedAt = transformDateTime(prop.SelectToken("$.updatedAt")?.ToString()) ;
            p.permitNumber = prop.SelectToken("$.permitNumber")?.ToString() ?? "N/A";
            p.whatsapp = prop.SelectToken("$.phoneNumber.whatsapp")?.ToString();
            Debug.WriteLine($"{p.Title},{p.phone},{p.agencyName},{p.completionStatus},{p.contactName},{p.location},{p.category}");
            bytes = 0; obj_cc = 0;
        }

        private string stringifyCategory(JToken categoryJObject)
        {
            if (categoryJObject == null) return "";
            
            try
            {
                /*var elems = categoryJObject.SelectTokens("$.[*]");
                IEnumerable<Tuple<int, string>> level_name_tuples =
                    elems.Select(el => new Tuple<int, string>(int.Parse(el.SelectToken("level").ToString()), el.SelectToken("name").ToString()));
                return string.Join(" > ", level_name_tuples.Select(e => e.Item2));*/
                var elems = categoryJObject.SelectTokens("$.[*]");
                return elems.Last().SelectToken("name")?.ToString() ?? "";
            }
            catch (Exception)
            {
                return "N/A";
            }
        }
        /// <summary>
        /// es
        /// </summary>
        /// <param name="m2"></param>
        /// <returns></returns>
        private string transformAreaToftsq(string m2)
        {
            if (string.IsNullOrWhiteSpace(m2)) return "";
            double m2_d = 0;
            if (!Double.TryParse(m2, out m2_d)) return "N/A";
             m2_d = m2_d * 10.764;
            return Math.Round(m2_d).ToString();
        }

        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);
        /// <summary>
        /// es
        /// </summary>
        /// <param name="ts">like 1 655 106 004</param>
        /// <returns></returns>
        private string transformDateTime(string ts)
        {
            if (string.IsNullOrWhiteSpace(ts)) return "";
            try{ return epoch.AddSeconds(int.Parse(ts)).ToShortDateString();}
            catch (Exception) { return ""; }
        }

        private string stringifyLocation(JToken locationJObject)
        {
            if (locationJObject == null) return "";

            try
            {
                var elems = locationJObject.SelectTokens("$.[*]");
                IEnumerable<Tuple<int, string>> level_name_tuples =
                    elems.Select(el => new Tuple<int, string>(int.Parse(el.SelectToken("level").ToString()), el.SelectToken("name").ToString()));
                return string.Join(", ", level_name_tuples.OrderByDescending(e=>e.Item1). Select(e => e.Item2));
            }
            catch (Exception)
            {
                return "N/A";
            }
        }




        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override Task RunConverter()
        {
            throw new NotImplementedException();
        }
        StringBuilder ReportBuilderTaskLevel = new StringBuilder();
        StringBuilder ReportBuilderPageLevel = new StringBuilder();
        StringBuilder ReportBuilderElementLevel = new StringBuilder();
        protected string TaskLockValue { get { return TargetPage; } }
        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        int total_count = 0;
        int global_couner = 0;
        public override async Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TaskLockValue])
            {
                ReportBuilderTaskLevel.Clear();
                TaskStatsInfo.Reset();
                OnStageChanged(ScrapTaskStage.DownloadingData);
                OnTaskDetailChanged("Downloading first page..");
                string raw;
                try
                {
                    raw = downloadOrRead(TargetPage, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
                    TaskStatsInfo.incSize(raw.Length);
                }
                catch (HttpRequestException)
                {
                    Stage = ScrapTaskStage.Failed;
                    OnStageChanged(Stage);
                    OnError("Network HttpRequestException error");
                    return;
                }
                catch (Exception err)
                {
                    Stage = ScrapTaskStage.Failed;
                    OnStageChanged(Stage);
                    OnError($"Error:{err.GetType().Name}:{err.Message}");
                    return;
                }
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(raw);
                if (!HasElementsTBF(doc.DocumentNode))
                {
                    return;
                }
                ResolvedTitle = GetPageUniqueUserTitle(doc.DocumentNode); ///+ ;
                if (string.IsNullOrWhiteSpace(ResolvedTitle)) throw new Exception("couldn't resolve page title");
                OnResolved(ResolvedTitle);
                try
                {
                    global_couner = 0;
                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle);
                    string outputPath = Path.Combine(Workspace.Current.CSVOutputFolder, uniqueOutputFileName + ".csv");
                    if (File.Exists(outputPath))
                    {
                        CoreUtils.RequestPrompt(new PromptContent($"file: '{Path.GetFileName(outputPath)}' {Environment.NewLine}{Environment.NewLine}If you want to preserve the old content please rename the file or make a copy of it."
                            , $"CSV file will be replaced!"
                            , new string[] { "Continue" }, PromptType.Warning), r => {
                                Debug.WriteLine(r);
                            });

                    }
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
                    OnIsStopEnabledd(true);
                    bool should_skip_areas_without_asking = false;
                    bool should_ask_skip_failed_elements_enumeration = true;
                    foreach (var page in EnumeratePages(TargetPage, ct))
                    {
                        ReportBuilderPageLevel.Clear();
                        OnPageStarted($"p {page.Item3}/{page.Item2}");
                        OnTaskDetailChanged($"Parsing page {page.Item3}");
                        Debug.WriteLine($"Enumerating CompactElements..");
                        Stopwatch sw = Stopwatch.StartNew();
                        List<Tuple<BProperty, string, JToken>> compactElements;
                        try
                        {
                            compactElements = EnumerateCompactElements(page.Item4).ToList();
                        }
                        catch (Exception err)
                        {
                            string u_response = "skip"; //default
                            CoreUtils.WriteLine($"EnumerateCompactElements: [{DateTime.Now}] at page:{page.Item3}, {err}");
                            if (should_ask_skip_failed_elements_enumeration)
                                CoreUtils.RequestPrompt(new PromptContent($"Page url: {page.Item5}{Environment.NewLine}{Environment.NewLine}(if you stop now {global_couner} of {total_count} properties will be saved){Environment.NewLine}Error details: '{err.Message}"
                            , $"Could not resolve page {page.Item3}!"
                            , new string[] { "Skip", "Skip all", "Stop task" }, PromptType.Error),
                            (response) => {
                                Debug.WriteLine("resp:" + response);
                                u_response = response;
                            });
                            if (u_response.ToLower() == "skip") continue;
                            else if (u_response.ToLower() == "skip all") { should_ask_skip_failed_elements_enumeration = false; continue; }
                            else if (u_response.ToLower() == "stop task") throw;
                            else continue;
                        }
                        List<BProperty> resolvedElements = new List<BProperty>(compactElements.Count);
                        var ect = sw.Elapsed;
                        Debug.WriteLine($"Enumerating CompactElements took {ect}");
                        int i = 0;
                        foreach (var item in compactElements)
                        {
                            ReportBuilderElementLevel.Clear();
                            ReportBuilderElementLevel.AppendLine($"element: i={i}, id={item.Item2} name= { item.Item1.Title }, agency_name= { item.Item1.agencyName }, phone= { item.Item1.phone }");
                            if (ct.IsCancellationRequested)
                            {
                                Debug.WriteLine("saving csv");
                                CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item3 > page.Item1);//start page is not current page
                                Debug.WriteLine("saved current page conent:" + outputPath);
                                Stage = ScrapTaskStage.Success;
                                OnStageChanged(Stage);
                                return;
                            }

                            int objs = 0;
                            long bytes = 0;
                            OnTaskDetailChanged($"{item.Item1.Title}/resolving details page");
                            retry_resolving:
                            bool should_retry = false;
                            try
                            {
                                ResolveElement(item.Item3, item.Item1, out objs, out bytes);
                            }
                            catch (Exception err)
                            {
                                string user_res = "Skip";
                                if (should_skip_areas_without_asking == false)
                                    CoreUtils.RequestPrompt(new PromptContent($"Do you want to skip them?{Environment.NewLine}{Environment.NewLine}(if you stop now {global_couner} of {total_count} properties will be saved){Environment.NewLine}Error detail: '{err.Message}'"
                                        , $"Resolving some fields filed for '{item.Item1.Title}'"
                                        , new string[] { "Skip", "Skip all", "Stop task" },
                                        PromptType.Question),
                                        res =>
                                        {
                                            user_res = res;
                                        });
                                if (user_res.ToLower() == "stop task")
                                {
                                    CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item3 > page.Item1);
                                    abortTask("task was ended", new List<BProperty>(), page.Item1, page.Item3, true);
                                    return;
                                }
                                else if (user_res.ToLower() == "skip")
                                {
                                    //skip
                                    should_retry = false;
                                }
                                else if (user_res.ToLower() == "skip all")
                                {
                                    //skip and never ask again
                                    should_retry = false;
                                    should_skip_areas_without_asking = true;
                                }
                               
                            }
                            if (should_retry) goto retry_resolving;

                            resolvedElements.Add(item.Item1);
                            TaskStatsInfo.incObject(objs);
                            TaskStatsInfo.incSize(bytes);
                            TaskStatsInfo.incElem(1);
                            i++;
                            global_couner++;
                            OnProgress(new DownloadingProg() { Total = total_count, Current = global_couner });
                        }
                        var rct = sw.Elapsed - ect;
                        Debug.WriteLine($"Resolving CompactElements took {rct}");
                        Debug.WriteLine("saving csv");
                        CSVUtils.CSVWriteRecords(outputPath, resolvedElements,page.Item3>page.Item1);
                        Debug.WriteLine("saved current page conent:" + outputPath);
                    }
                    OnTaskDetailChanged(null);
                    OnStageChanged(ScrapTaskStage.Success);
                    return;

                }
                catch (Exception err)
                {
                    CoreUtils.WriteLine($"unknown error [{DateTime.Now}]{Environment.NewLine} {err}");
                    OnStageChanged(ScrapTaskStage.Failed);
                    OnError($"Error: '{err.Message}'");
                    return;
                }


            }
        }

    }
}
