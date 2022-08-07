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
        /// <summary>
        /// the first resolving stage: only the pieces of information that are visible at the target page itself are parsed, further heavy resolving that requires element-wise navigations should be carried out at the ResolveElement method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNode">the target page document node</param>
        /// <returns>collection of elements at their compact phase</returns>
        public abstract IEnumerable<T> EnumerateCompactElements<T>(HtmlNode pageNode);
        /// <summary>
        /// the second (and last) resolving phase, this may involve performing one or more web requests, it may also not be required at all in this case it should simply return without affecting the element
        /// </summary>
        /// <param name="compactElement">the compact elements: same object as the final element when not all fields are populated yet and / or not all objects are downloded locally, it must contain the minimum information required for full resolving e,g an element-specific url</param>
        /// <param name="bytes">estimation of number of bytes written to disk during the resolving process, this is used for user infomation</param>
        /// <param name="obj_cc">number of files added under the workspace's objects directory and sub directories (html files, images etc) this is used for user infomation</param>
        public abstract void ResolveElement(object compactElement, out int bytes, out int obj_cc);

        /// <summary>
        /// the User implies user friendly piece of information that has well formatted text
        /// </summary>
        /// <returns></returns>
        /// 
        public abstract string GetElementUniqueID(HtmlNode elementRootNode);//from targetpage and only,(compact elem enumerating phase)
        /// <summary>
        /// not used yet
        /// </summary>
        public abstract string GetElementUserID(HtmlNode elementRootNode);
        /// <summary>
        /// not used yet
        /// </summary>
        public abstract string GetElementUserUniqueID(HtmlNode elementRootNode);
        /// <summary>
        /// not used yet
        /// </summary>
        /// <returns></returns>
        public abstract string GetPageUserTitle(HtmlNode pageNode);
        /// <summary>
        /// not used yet
        /// </summary>
        public abstract string GetPageUniqueUserTitle(HtmlNode pageNode);
        /// <summary>
        /// not used yet
        /// </summary>
        public abstract string GetPageUniqueID(HtmlNode pageNode);
        public abstract bool HasElementsTBF(HtmlNode pageNode);
        /// <summary>
        /// used as part of the UI displayed task details while resolving the element, this should siimply return a suitable property e,g name or url
        /// </summary>
        /// <returns></returns>
        public abstract string GetElementTaskDetailHint(object elem);
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
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
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