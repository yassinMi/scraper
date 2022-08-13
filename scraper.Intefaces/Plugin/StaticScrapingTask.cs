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
        /// property names convention: the suffix "User" implies that the property is visible to the user
        /// </summary>
        /// <returns></returns>
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
        /// used to resolve the title and determine file name, this has precedence over the GetPageUniqueID, at least one must return a valid string
        /// </summary>
        public virtual string GetPageUniqueUserTitle(HtmlNode pageNode) { return null; }
        /// <summary>
        /// used to resolve the title and determine file name, GetElementUserUniqueID has precedence over this, at least one must return a valid string
        /// </summary>
        public virtual string GetPageUniqueID(HtmlNode pageNode) { return TargetPage; }
        public abstract bool HasElementsTBF(HtmlNode pageNode);
        /// <summary>
        /// (todo) used to display task details while resolving the element, this should return a string that tells the user what elemnt is currently being resolved e,g a name or url property value, returning null hides the detail (default)
        /// </summary>
        /// <returns></returns>
        public virtual string GetElementTaskDetailHint(object elem) { return null; }

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
                catch (Exception err)
                {
                    Debug.WriteLine($"downloadOrRead throw {err.Message}");
                    throw;
                }

            }
        }
        /// <summary>
        /// the value used to lock running scraper, usually the target page ptr (url) should be used to prevent starting concurrent tasks that wrte to the same output files
        /// </summary>
        protected virtual string TaskLockValue { get { return TargetPage; } }


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
                ResolvedTitle = GetPageUniqueUserTitle(doc.DocumentNode)?? GetPageUniqueID(doc.DocumentNode); ///+ ;
                if (string.IsNullOrWhiteSpace(ResolvedTitle)) throw new Exception("couldn't resolve page title");
                OnResolved(ResolvedTitle);
                try
                {

                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle) + ".csv";
                    var outputPath = Path.Combine(Workspace.Workspace.Current.CSVOutputFolder, uniqueOutputFileName);
                    ActualOutputFile = DesiredOutputFile ?? outputPath;
                    foreach (var page in EnumeratePages(TargetPage))
                    {
                        OnPageStarted($"[page {page.Item1}/{page.Item2}]");
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
                        var rct = sw.Elapsed-ect;
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

    }

}