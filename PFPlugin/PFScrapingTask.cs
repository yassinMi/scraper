#define LOOP_IN_FIRST_PAGE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PFPlugin.Model;
using scraper.Core.UI;

namespace PFPlugin
{
    public class PFScrapingTask : ScrapingTaskBase
    {
        public PFScrapingTask(string tp)
        {
            TargetPage = tp;
        }

        public static string downloadOrRead(string pageLink, string folder, bool force_update=false)
        {
            Debug.WriteLine($"downloadOrRead '{pageLink}'");
            string uniqueFilename = Path.Combine(folder, CoreUtils.getUniqueLinkHash(pageLink) + ".html");
            if (force_update ==false &&File.Exists(uniqueFilename)) { return File.ReadAllText(uniqueFilename); }
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

        StringBuilder ReportBuilderTaskLevel = new StringBuilder();
        StringBuilder ReportBuilderPageLevel = new StringBuilder();
        StringBuilder ReportBuilderElementLevel = new StringBuilder();

        /// <summary>
        /// handle all fatal error by calling this, it will dump file, and warn the user and set task status.
        /// </summary>
        /// <param name="dump"></param>
        /// <param name="silent">not saving dump and not showing message box,</param>
        private void abortTask(string hint, List<Agent> dump, int start_page, int end_page, bool silent = false)
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
                    CoreUtils.RequestPrompt(new PromptContent($"{hint}{Environment.NewLine}{dump.Count} agents records at {saved_pages_range_str} are saved here:{dump_file}", "Task aborted", new string[] { "OK" }, PromptType.Error), s => { });

            }

            else
            {
                //# nothing to save
                CoreUtils.WriteLine($"aborted: [{DateTime.Now}], with no records");
                if (silent == false)
                    CoreUtils.RequestPrompt(new PromptContent($"{hint}{Environment.NewLine}{"no agents records were collected."}", "Task aborted", new string[] { "OK" }, PromptType.Error), s => { });
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

        /// <summary>
        /// agents listing payload
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        string getJsonAgentsListingPayload(string doc)
        {
            //between  window.propertyfinder.settings.agent =  .. window.propertyfinder.settings.gtm 
            string d1 = "window.propertyfinder.settings.agent =";
            string d2 = "window.propertyfinder.settings.gtm =";
            var first_ix = doc.IndexOf(d1);
            var last_ix = doc.IndexOf(d2);
            string res = doc.Substring(first_ix + d1.Length, last_ix - (first_ix + d1.Length))
                .Trim().Trim(';').Trim('{','}').Trim().TrimEnd(',').Substring(8);

            Debug.WriteLine($"yass{res}yass");
            return res;
        }
        /// <summary>
        /// agent details payload
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        string getJsonAgentDetailsPayload(string doc)
        {
            string d1 = "window.propertyfinder.settings.property = {";
            string d2 = " form: ";
            var first_ix = doc.IndexOf(d1);
            var last_ix = doc.IndexOf(d2);
            string res = doc.Substring(first_ix + d1.Length, last_ix - (first_ix + d1.Length))
                .Trim().TrimEnd(',');
            string d1_ = "payload: ";
            var first_ix_ = res.IndexOf(d1_);
            res = res.Substring(first_ix_ + d1_.Length);
            Debug.WriteLine($"yass{res}yass");
            return res;
        }
        int total_count = 0;
        /// <summary>
        /// tuple: agent compact, profle like, since we're not having links as model fields
        /// links are made absolute 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public  IEnumerable<Tuple<Agent,string>> EnumerateCompactElements(string doc)
        {
            var json = getJsonAgentsListingPayload(doc);
            //var obj = JsonConvert.DeserializeObject<dynamic>(json);
            JObject j = JObject.Parse(json);
            var meta = j.SelectToken("$.meta");
            if (total_count == 0)
            {
                int.TryParse(meta.SelectToken("$.total_count").ToString(), out total_count);

            }
            Debug.WriteLine(meta);
            var elems = j.SelectTokens("$.data[*]");
            Debug.WriteLine(elems.Count());
            var included_languages = j.SelectTokens("$.included[?(@.type=='language')]");
            Debug.WriteLine(included_languages.Count());
            var included_brokers = j.SelectTokens("$.included[?(@.type=='broker')]");
            Debug.WriteLine(included_brokers.Count());
            foreach (var agent in elems) {
                Agent a = new Agent();
                a.Name = agent.SelectToken("$.attributes.name").ToString();
                a.CompanyName = agent.SelectToken("$.meta.broker_name").ToString();
                a.Phone = agent.SelectToken("$.attributes.phone").ToString();
                a.TotalProperties = agent.SelectToken("$.attributes.total_properties").ToString();
                //a.Nationality = agent.SelectToken("$.attributes.nationality").ToString();
                a.position = agent.SelectToken("$.attributes.position").ToString(); //7
                a.Image = agent.SelectToken("$.attributes.image_token").ToString();
                a.Country = agent.SelectToken("$.attributes.country_name").ToString();
                //a.Company = "N/A";
                //a.isTrusted = agent.SelectToken("$.attributes.is_trusted").ToString();
                //a.WhatsappResponseTime = agent.SelectToken("$.attributes.whatsapp_response_time_readable").ToString();
                a.YearsOfExperience = agent.SelectToken("$.attributes.years_of_experience").ToString();
                a.LinkedinAddress= agent.SelectToken("$.attributes.linkedin_address").ToString();
                string brokey_id = agent.SelectToken("$.meta.broker_id").ToString();
                Debug.WriteLine(brokey_id);
                string profile_link = agent.SelectToken("$.links.profile").ToString();
                string absolute_profile_link = "https://www.propertyfinder.ae" + profile_link;

                try
                {
                    a.CompanyAddress = j.SelectToken($"$.included[?(@.type=='broker' && @.id=='{brokey_id}')].attributes.address").ToString().Replace("\r\n",", ");
                }
                catch 
                {      
                                  
                }
                Debug.WriteLine($"{a.Name},{a.CompanyName},{a.Phone},{a.TotalProperties},{a.Country},{a.position},{a.Country}");
                yield return new Tuple<Agent, string>(a, absolute_profile_link);
            }
            

            
            
        }
        /// <summary>
        /// can throw exception when user don't press retry
        /// </summary>
        /// <param name="rootPageUrl"></param>
        /// <returns></returns>
        public  IEnumerable<Tuple<int, int, HtmlNode,string>> EnumeratePages(string rootPageUrl)
        {
            string pageRaw;
            HtmlDocument doc = new HtmlDocument();
            retry:
            try
            {
                pageRaw = downloadOrRead(rootPageUrl, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy== CachePolicy.ElementsPagesOnly)|| (UserSettings.Current.CachePolicy== CachePolicy.None));
                doc.LoadHtml(pageRaw);
            }
            catch (Exception err)
            {

                string u_response = "CANCEl";
                CoreUtils.WriteLine($"EnumeratePages: [{DateTime.Now}] {rootPageUrl}, {err}");
                CoreUtils.RequestPrompt(new PromptContent($"Couldn't fetch page '{rootPageUrl}', '{err.Message}'{Environment.NewLine}press Ok to retry{Environment.NewLine}Press Cancel to abort task", "error", new string[] { "OK", "CANCEL" }, PromptType.Error),
                    (response) => {
                        Debug.WriteLine("resp:" + response);
                        u_response = response;
                    });
                if (u_response == "OK") goto retry;
                else throw;
            }
           
            
            int min = 1;
            int max = 1;
            if (isMultiplePagesAgentsListings(doc.DocumentNode))
            {
                max = getLastPageNumber(pageRaw);
                CoreUtils.WriteLine($"max page:" + max);
            }
            foreach (var pg in Enumerable.Range(min, max))
            {
                string pageLink = AppendPageNumToUrl(ClearPageNumFromUrl(rootPageUrl), pg);
                string raw;
                try
                {
#if LOOP_IN_FIRST_PAGE //looping at the first page only (where elements are pre dowloaded in the test ws
                    raw = downloadOrRead(rootPageUrl, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
#else
                    raw = downloadOrRead(pageLink, Workspace.Current.TPFolder, (UserSettings.Current.CachePolicy == CachePolicy.ElementsPagesOnly) || (UserSettings.Current.CachePolicy == CachePolicy.None));
#endif

                }
                catch (Exception err)
                {
                    string u_response = "CANCEl";
                    CoreUtils.WriteLine($"EnumeratePages: [{DateTime.Now}] {pageLink}, {err}");
                    CoreUtils.RequestPrompt(new PromptContent($"Couldn't fetch page '{pageLink}', '{err.Message}'{Environment.NewLine}press Ok to retry{Environment.NewLine}Press Cancel to abort task", "error", new string[] { "OK", "CANCEL" }, PromptType.Error),
                        (response) => {
                            Debug.WriteLine("resp:" + response);
                            u_response = response;
                        });
                    if (u_response == "OK") goto retry;
                    else throw;
                }
                TaskStatsInfo.incSize(raw.Length);
                HtmlDocument newDoc = new HtmlDocument();
                newDoc.LoadHtml(raw);
                yield return new Tuple<int, int, HtmlNode,string>(pg, max, newDoc.DocumentNode,raw);
            }

        }

        private string ClearPageNumFromUrl(string targetRootPage)
        {
            Debug.WriteLine($"ClearPageNumFromUrl : '{targetRootPage}");
            //https://www.propertyfinder.ae/en/find-agent/search?page=1
            //https://www.propertyfinder.ae/en/find-agent/search?
            //https://www.propertyfinder.ae/en/find-agent/search?category_id=1&order_by=-trusted_score
            //https://www.propertyfinder.ae/en/find-agent/search?category_id=1&order_by=-trusted_score&page=2
            string s1 = Regex.Replace(targetRootPage, @"&page=\d+", "");
            string s2= Regex .Replace(s1, @"\?page=\d+$", "");
            string res=  Regex.Replace(s2, @"page=\d+", "");
            Debug.WriteLine($"res : '{res}");
            return res;

        }

        private string AppendPageNumToUrl(string p, int pg)
        {
            Debug.WriteLine($"AppendPageNumToUrl : '{p}");
            string res;
            Uri u;
            Uri.TryCreate(p, UriKind.Absolute, out u);
            if (string.IsNullOrWhiteSpace(u.Query ))
            {
                res= u.OriginalString + $"?page={pg}";
            }
            else
            {
                res= u.OriginalString+ $"&page={pg}";
            }

            Debug.WriteLine($"res : '{res}");
            return res;

        }

        private int getLastPageNumber(string document)
        {
            var m = Regex.Match(document, @"""page_count"":(\d+)");
            if (m.Success == false)
            {
                throw new Exception("cound find last page");
            }
            return int.Parse(m.Groups[1].Value);
        }

        private bool isMultiplePagesAgentsListings(HtmlNode documentNode)
        {
            return true;//todo
        }

        private bool isNoneEmptyAgentsListings(HtmlNode documentNode)
        {
            return true;
        }

        private bool isAgentsListings(HtmlNode documentNode)
        {
            return true;
        }

        public  string GetElementUniqueID(HtmlNode elementRootNode)
        {
            return "lk";
        }

        public  string GetElementUserID(HtmlNode elementRootNode)
        {
            return "k";
        }

        public  string GetElementUserUniqueID(HtmlNode elementRootNode)
        {
            return "oj";
        }

        public  string GetPageUniqueUserTitle(HtmlNode pageNode)
        {
            //main//div[@data-qs='agent-list']//h1
            var h1 = pageNode.SelectSingleNode("//main//div[@data-qs='agent-list']//h1");
            return h1.InnerText;
        }

        public  bool HasElementsTBF(HtmlNode pageNode)
        {
            return true;
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }


        public string getAreas(JObject j)
        {


            var locations_comunity = j.SelectTokens("$.included[?(@.type=='location' && @.attributes.location_type=='COMMUNITY')].attributes.name");
            return string.Join(" • ", locations_comunity.Select(l => l.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="compactElement">string being absolute profile link</param>
        /// <param name="bytes"></param>
        /// <param name="obj_cc"></param>
        public  void ResolveElement(Tuple<Agent,string> compactElement, out int bytes, out int obj_cc)
        {
            string raw_profile = downloadOrRead(compactElement.Item2, Workspace.Current.HtmlObjectsFolder,  UserSettings.Current.CachePolicy == CachePolicy.None);
            var json = getJsonAgentDetailsPayload(raw_profile);
            JObject j = JObject.Parse(json);

            compactElement.Item1.Areas = getAreas(j);
            bytes = raw_profile.Length; obj_cc = 1;
            return;
        }
        protected  string TaskLockValue { get { return TargetPage; } }


        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        public override async Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TaskLockValue])
            {
                ReportBuilderTaskLevel.Clear();
                TaskStatsInfo.Reset();
                OnStageChanged(ScrapTaskStage.DownloadingData);
                OnTaskDetailChanged("Getting page..");
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
                    int global_couner = 0;
                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle);
                    string outputPath = Path.Combine(Workspace.Current.CSVOutputFolder, uniqueOutputFileName+ ".csv") ;
                    if (File.Exists(outputPath))
                    {
                        CoreUtils.RequestPrompt(new PromptContent($"If you want to preserve the old content rename the file or make a copy of it."
                            , $"CSV file '{Path.GetFileName(outputPath)}' will be erased!"
                            , new string[] { "Override" }, PromptType.Warning), r => {
                            Debug.WriteLine(r);
                        });

                    }
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
                    OnIsStopEnabledd(true);
                    
                    foreach (var page in EnumeratePages(TargetPage))
                    {
                        ReportBuilderPageLevel.Clear();
                        OnPageStarted($"p {page.Item1}/{page.Item2}");
                        OnTaskDetailChanged($"parsing page {page.Item1}");
                        Debug.WriteLine($"Enumerating CompactElements..");
                        Stopwatch sw = Stopwatch.StartNew();
                        var compactElements = EnumerateCompactElements(page.Item4).ToList();
                        List<Agent> resolvedElements = new List<Agent>(compactElements.Count);
                        var ect = sw.Elapsed;
                        Debug.WriteLine($"Enumerating CompactElements took {ect}");
                        int i = 0;
                        foreach (var item in compactElements)
                        {
                            ReportBuilderElementLevel.Clear();
                            ReportBuilderElementLevel.AppendLine($"element: i={i}, profile={item.Item2} name= { item.Item1.Name }, comp_name= { item.Item1.CompanyName }, phone= { item.Item1.Phone }");
                            if (ct.IsCancellationRequested)
                            {
                                Debug.WriteLine("saving csv");
                                CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                                Debug.WriteLine("saved current page conent:" + outputPath);
                                Stage = ScrapTaskStage.Success;
                                OnStageChanged(Stage);
                                return;
                            }

                            int objs=0, bytes = 0;
                            OnTaskDetailChanged($"{item.Item1.Name}/downloading details page");
                            retry_resolving:
                            bool should_retry = false;
                            try
                            {
                                ResolveElement(item, out bytes, out objs);
                            }
                            catch (Exception)
                            {
                                string user_res = "CANCEL";
                                CoreUtils.RequestPrompt(new PromptContent($"Do you want to skip it?{Environment.NewLine}{Environment.NewLine}(if you stop {global_couner} of {total_count} agents will be saved)"
                                    , $"{(nameof(Agent.Areas))} could not be resolved for '{item.Item1.Name}'" , new string[] { "Skip", "Retry", "Stop task" },
                                    PromptType.Question),
                                    res =>
                                    {
                                        user_res = res;
                                    });
                                if (user_res.ToLower() == "stop task")
                                {
                                    CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                                    abortTask("task was ended", new List<Agent>(),1,page.Item2,true);
                                    return;
                                }
                                else if(user_res.ToLower() == "skip")
                                {
                                    //skip
                                    should_retry = false;
                                }
                                else if (user_res.ToLower() == "retry")
                                {
                                    should_retry = true;
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
                        CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
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
        public override Task RunConverter()
        {
            throw new NotImplementedException();
        }
    }
}
