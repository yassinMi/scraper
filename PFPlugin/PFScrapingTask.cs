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

namespace PFPlugin
{
    public class PFScrapingTask : StaticScrapingTask
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
        public override IEnumerable<T> EnumerateCompactElements<T>(HtmlNode pageNode)
        {
            yield break;
        }

        public override IEnumerable<Tuple<int, int, HtmlNode>> EnumeratePages(string rootPageUrl)
        {
            yield break;
        }

        public override string GetElementUniqueID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public override string GetElementUserID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public override string GetElementUserUniqueID(HtmlNode elementRootNode)
        {
            throw new NotImplementedException();
        }

        public override bool HasElementsTBF(HtmlNode pageNode)
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override void ResolveElement(object compactElement, out int bytes, out int obj_cc)
        {
            throw new NotImplementedException();
        }

        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        public override async Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TaskLockValue])
            {
                TaskStatsInfo.Reset();
                OnStageChanged(ScrapTaskStage.DownloadingData);
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
                ResolvedTitle = GetPageUniqueUserTitle(doc.DocumentNode) ?? GetPageUniqueID(doc.DocumentNode); ///+ ;
                if (string.IsNullOrWhiteSpace(ResolvedTitle)) throw new Exception("couldn't resolve page title");
                OnResolved(ResolvedTitle);
                try
                {

                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle) + ".csv";
                    var outputPath = Path.Combine(Workspace.Current.CSVOutputFolder, uniqueOutputFileName);
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
                    foreach (var page in EnumeratePages(TargetPage))
                    {
                        OnPageStarted($"[page {page.Item1}/{page.Item2}]");
                        Debug.WriteLine($"Enumerating CompactElements..");
                        Stopwatch sw = Stopwatch.StartNew();
                        var compactElements = EnumerateCompactElements<object>(page.Item3).ToList();
                        List<object> resolvedElements = new List<object>(compactElements.Count);
                        var ect = sw.Elapsed;
                        Debug.WriteLine($"Enumerating CompactElements took {ect}");
                        int i = 0;
                        foreach (var item in compactElements)
                        {
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
                            OnProgress(new DownloadingProg() { Total = compactElements.Count, Current = i });
                            OnTaskDetailChanged($"Collecting {"Element"} info: {GetElementTaskDetailHint(item)}");
                        }
                        var rct = sw.Elapsed - ect;
                        Debug.WriteLine($"Resolving CompactElements took {rct}");
                        Debug.WriteLine("saving csv");
                        CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                        Debug.WriteLine("saved current page conent:" + outputPath);
                    }

                    OnStageChanged(ScrapTaskStage.Success);
                    return;

                }
                catch
                {
                    OnStageChanged(ScrapTaskStage.Failed);
                    OnError("Network Error");
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
