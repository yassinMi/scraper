using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using scraper.Core.Utils;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace scraper.Core
{
    public abstract class StaticScrapingTask : ScrapingTaskBase
    {
        public abstract IEnumerable<Tuple<int, int, HtmlNode>> EnumeratePages(string rootPageUrl);
        public abstract IEnumerable<T> EnumerateCompactElements<T>(HtmlNode pageNode);
        public abstract void ResolveElement(object compactElement, out int bytes, out int obj_cc);

        /// <summary>
        /// the User implies user friendly piece of information that has well formatted text
        /// </summary>
        /// <returns></returns>
        /// 
        public abstract string GetElementUniqueID(HtmlNode elementRootNode);//from targetpage and only,(compact elem enumerating phase)
        public abstract string GetElementUserID(HtmlNode elementRootNode);
        public abstract string GetElementUserUniqueID(HtmlNode elementRootNode);
        public abstract string GetPageUserTitle(HtmlNode pageNode);
        public abstract string GetPageUniqueUserTitle(HtmlNode pageNode);
        public abstract string GetPageUniqueID(HtmlNode pageNode);
        public abstract bool HasElementsTBF(HtmlNode pageNode);
        /// <summary>
        /// used as part of the UI displayed task details while resolving the element, this should siimply return a suitable property e,g name or url
        /// </summary>
        /// <returns></returns>
        public abstract bool GetElementTaskDetailHint(object elem);
        /// <summary>
        /// used in task details ,nd 
        /// </summary>
        /// <param name="compactElement"></param>
        /// <returns></returns>
        public abstract string GetElementHint(object compactElement);

        public static string downloadOrRead(string pageLink, string folder)
        {
            Debug.WriteLine("downloadOrRead ");
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
                catch (Exception)
                {
                    Debug.WriteLine("downloadOrRead trwoing");
                    throw;
                }

            }
        }
        /// <summary>
        /// the value used to lock running scraper, usually the target page ptr (url) should be used to prevent starting concurrent tasks that wrte to the same output files
        /// </summary>
        public abstract string TaskLockValue { get; }


        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        public override async  Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TaskLockValue])
            {
                TaskStatsInfo.Reset();
                OnStageChanged(ScrapTaskStage.DownloadingData);
                string raw;
                try
                {
                    raw = downloadOrRead(TargetPage, Workspace.Workspace.Current.TPFolder);
                    TaskStatsInfo.incSize(raw.Length);
                }
                catch (HttpRequestException)
                {
                    Stage = ScrapTaskStage.Failed;
                    OnStageChanged(Stage);
                    OnError("Network error");
                    return;
                }
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(raw);
                if (!HasElementsTBF(doc.DocumentNode))
                {
                    return;
                }
                ResolvedTitle = GetPageUniqueUserTitle(doc.DocumentNode); ///+ ;
                OnResolved(ResolvedTitle);
                try
                {

                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle) + ".csv";
                    var outputPath = Path.Combine(Workspace.Workspace.Current.CSVOutputFolder, uniqueOutputFileName);

                    foreach (var page in EnumeratePages(TargetPage))
                    {
                        OnPageStarted($"[page {page.Item1}/{page.Item2}]");
                        var compactElements = EnumerateCompactElements<object>(page.Item3).ToList();
                        List<object> resolvedElements = new List<object>(compactElements.Count);
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

    }

}