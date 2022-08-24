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

        string getJsonPayload(string doc)
        {
            //between  window.propertyfinder.settings.agent =  .. window.propertyfinder.settings.gtm 
            string d1 = "window.propertyfinder.settings.agent =";
            string d2 = "window.propertyfinder.settings.gtm";
            var first_ix = doc.IndexOf(d1);
            var last_ix = doc.IndexOf(d2);
            string res = doc.Substring(first_ix + d1.Length, last_ix - (first_ix + d1.Length))
                .Trim().Trim(';').Trim('{','}').Trim().TrimEnd(',').Substring(8);

            Debug.WriteLine($"yass{res}yass");
            return res;
        }
        int total_count = 0;
        public  IEnumerable<Model.Agent> EnumerateCompactElements(string doc)
        {
            var json = getJsonPayload(doc);
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
                a.BrokerName = agent.SelectToken("$.meta.broker_name").ToString();
                a.Phone = agent.SelectToken("$.attributes.phone").ToString();
                a.TotalProperties = agent.SelectToken("$.attributes.total_properties").ToString();
                a.Nationality = agent.SelectToken("$.attributes.nationality").ToString();
                a.position = agent.SelectToken("$.attributes.position").ToString(); //7
                a.Image = agent.SelectToken("$.attributes.image_token").ToString();
                a.Country = agent.SelectToken("$.attributes.country_name").ToString();
                a.Company = "N/A";
                a.isTrusted = agent.SelectToken("$.attributes.is_trusted").ToString();
                a.WhatsappResponseTime = agent.SelectToken("$.attributes.whatsapp_response_time_readable").ToString();
                a.YearsOfExperience = agent.SelectToken("$.attributes.years_of_experience").ToString();
                a.LinkedinAddress= agent.SelectToken("$.attributes.linkedin_address").ToString();
                Debug.WriteLine($"{a.Name},{a.BrokerName},{a.Phone},{a.TotalProperties},{a.Nationality},{a.position},{a.Country},{a.WhatsappResponseTime}");
                yield return a;
            }
            

            
            
        }

        public  IEnumerable<Tuple<int, int, HtmlNode,string>> EnumeratePages(string rootPageUrl)
        {
            string pageRaw = downloadOrRead(rootPageUrl, Workspace.Current.TPFolder);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageRaw);
            var pagination_container = doc.DocumentNode.SelectSingleNode("//div[@class='pagination__links']");

            if ((isAgentsListings(doc.DocumentNode) == false) || (isNoneEmptyAgentsListings(doc.DocumentNode) == false))
            {
                yield break;
            }

            int min = 1;
            int max = 1;
            if (isMultiplePagesAgentsListings(doc.DocumentNode))
            {
                max = getLastPageNumber(pageRaw);
            }
            foreach (var pg in Enumerable.Range(min, max))
            {

                string pageLink = AppendPageNumToUrl(ClearPageNumFromUrl(rootPageUrl), pg);
                string raw =  downloadOrRead(pageLink, Workspace.Current.TPFolder);
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

        public  void ResolveElement(object compactElement, out int bytes, out int obj_cc)
        {
            bytes = 0; obj_cc = 0;
            return;
        }
        protected  string TaskLockValue { get { return TargetPage; } }


        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        public override async Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TaskLockValue])
            {
                TaskStatsInfo.Reset();
                OnStageChanged(ScrapTaskStage.DownloadingData);
                OnTaskDetailChanged("Getting page..");
                string raw;
                try
                {
                    raw = downloadOrRead(TargetPage, Workspace.Current.TPFolder);
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
                        CoreUtils.RequestPrompt(new PromptContent($"CSV file '{outputPath}' is about to be erased.{Environment.NewLine}If you want to keep the old content please rename the file or make a copy of it before proceeding.{Environment.NewLine}Click OK to continue","Warning", new string[] { "OK" }, PromptType.Warning), r => {
                            Debug.WriteLine(r);
                        });

                    }
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
                    foreach (var page in EnumeratePages(TargetPage))
                    {

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
                            Task.Delay(4).GetAwaiter().GetResult();
                            if (ct.IsCancellationRequested)
                            {
                                Debug.WriteLine("saving csv");
                                CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                                Debug.WriteLine("saved current page conent:" + outputPath);
                                Stage = ScrapTaskStage.Success;
                                OnStageChanged(Stage);
                                return;
                            }

                            int objs, bytes = 0;
                            ResolveElement(item, out bytes, out objs);
                            resolvedElements.Add(item);
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
                    OnError("Unknows Error");
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
