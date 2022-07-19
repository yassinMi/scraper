using CsvHelper;
using HtmlAgilityPack;
using Mi.Common;
using scraper.Core;
using scraper.Model;
using scraper.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Plugin
{
    public class BusinessCompact
    {
        public string name , link, address, desc, thumbnail;

    }
    public class BLScraper : IPlugin
    {
        public IElementDescription ElementDescription
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// needded by the plugin to save objects and output files
        /// </summary>
        public string WorkspaceDirectory {get;set;}

        public string ElementName { get{return "Business";}}
        public string ElementNamePlural{get{return "Businesses";} }

        public string Name { get{return "Businesslist.ph scraper"; } }

        public Version Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        public IPluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public IPluginScrapingTask GetTask(string targetPage)
        {
            return new BLScrapingTask() { TargetPage = targetPage, WorkspaceDirectory = this.WorkspaceDirectory };
        }
    }

    public class BLScrapingTask : IPluginScrapingTask
    {
        /// <summary>
        /// only to be instantiated through the IPlugin instance (using IPlugin.getScrapingTask)
        /// </summary>
        public BLScrapingTask()
        {

        }

        public string WorkspaceDirectory { get; set; }

        public DownloadingProg DownloadingProgress { get; set; }


        public string ResolvedTitle { get; set; } = null;

        public ScrapTaskStage Stage { get; set; }

        public string TargetPage { get; set; } = null;

        public event EventHandler<string> OnError;
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnResolved;
        public event EventHandler<ScrapTaskStage> OnStageChanged;
        public event EventHandler<string> OnTaskDetail;

        public void Pause()
        {
            throw new NotImplementedException();
        }

        async public Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            this.OnStageChanged?.Invoke(this, this.Stage);

        }


        IEnumerable<Tuple<string,string>> getInfos(HtmlNode elementPage)
        {
            foreach (var item in elementPage.SelectNodes("//div[@class='info']"))
            {
                int cc = item.ChildNodes.Count;
                string key = item.FirstChild?.InnerText;
                string Formattedvalue = "";
                if (cc == 2)
                {
                    Formattedvalue = item.ChildNodes[1].InnerHtml;
                }
                else if (cc == 1)
                {
                    Debug.WriteLine("cc=1: " + item.FirstChild.InnerHtml);
                }
                yield return new Tuple<string, string>(key, item.FirstChild?.InnerText);
            }
        }
        string getPhoneNumber(HtmlNode elementPage)
        {
            return elementPage.SelectSingleNode("//div[@class='text phone']")?.InnerText;
        }
        string getEmail(HtmlNode elementPage)
        {
            return "unr";
        }
        string getWebsite(HtmlNode elementPage)
        {
            return "unr";
        }
        string getEmployees(HtmlNode elementPage)
        {
            return "unr";
        }

        public HtmlNode getget()
        {
            return new HtmlNode(HtmlNodeType.Element, new HtmlDocument(),5);
        }

        /// <summary>
        /// only required by downloadOrReadFromObject, returns a filesystem-friendly hash
        /// </summary>
        string getUniqueLinkHash(string businessLink)
        {
            return Utils. CreateMD5(businessLink);
        }

        /// <summary>
        /// if the object exists: reads and returns html string, no web requests
        /// else: download and save object named after the url hash, and then return html string
        /// aka provides caching functionality
        /// </summary>
        /// <param name="businessLink"></param>
        public string downloadOrReadFromObject(string businessLink)
        {
            Debug.WriteLine("calculating unique html name");
            string uniqueFilename = Path.Combine(WorkspaceDirectory, ConfigService .Instance.ProductsHTMLRelativeLocation,getUniqueLinkHash(businessLink) + ".html")  ;
            if (File.Exists(uniqueFilename))
            {
                //load
                Debug.WriteLine("exists");

                return File.ReadAllText(uniqueFilename);
            }
            else
            {
                //download
                Debug.WriteLine("new");
                string rawElementPage = WebHelper.instance.GetPageTextSync(businessLink);
                File.WriteAllText(uniqueFilename, rawElementPage);
                return rawElementPage;

            }

        }

        public string downloadOrReadFromTargetPageObject(string businessLink)
        {
            Debug.WriteLine("calculating unique html name");
            string uniqueFilename = Path.Combine(WorkspaceDirectory, ConfigService.Instance.TargetPagesRelativeLocation, getUniqueLinkHash(businessLink) + ".html");
            if (File.Exists(uniqueFilename))
            {
                //load
                Debug.WriteLine("exists");

                return File.ReadAllText(uniqueFilename);
            }
            else
            {
                //download
                Debug.WriteLine("new");
                string rawElementPage = WebHelper.instance.GetPageTextSync(businessLink);
                File.WriteAllText(uniqueFilename, rawElementPage);
                return rawElementPage;

            }

        }

        /// <summary>
        /// the missing fields are:  phone email website, employees
        /// existing fields are  name, desc, thumb, location, link
        /// </summary>
        /// <param name="compactElement"></param>
        private void resolveFullElement(Business compactElement)
        {
            Debug.WriteLine("downloading or reading html link: "+ compactElement.link);
            
            string rawElementPage = downloadOrReadFromObject(compactElement.link);
            HtmlDocument elementDoc = new HtmlDocument();
            elementDoc.LoadHtml(rawElementPage);
            Debug.WriteLine("resolving missing fields from html");
            compactElement.phonenumber = getPhoneNumber(elementDoc.DocumentNode);
            compactElement.email = getEmail(elementDoc.DocumentNode);
            Debug.WriteLine("missing fields 2");
            compactElement.website = getWebsite(elementDoc.DocumentNode);
            compactElement.employees = getEmployees(elementDoc.DocumentNode);
            Debug.WriteLine("done");


        }

        

        public static string getAddress(HtmlNode node)
        {
            var div = node.SelectSingleNode(".//div[@class='address']");
            if (div == null) return null;
            if  (string.IsNullOrWhiteSpace(div.InnerHtml) ) return null;
            return Utils. StripHTML(div.InnerHtml);
        }
        public static string getName(HtmlNode node)
        {
            return node.SelectSingleNode("./h4//a")?.InnerHtml;
        }
        public static string getLink(HtmlNode node)
        {
            return node.SelectSingleNode(".//h4//a")?.GetAttributeValue<string>("href", null);
        }
        public static  string getDesc(HtmlNode node)
        {
            return "unresolved";
        }
        public static  string getThumb(HtmlNode node)
        {
            var img = node.SelectSingleNode(".//div[@class='details']//a//span[@class='logo']//img");
            return (img != null) ? img.GetAttributeValue("src", "") : null;
        }

        public static IEnumerable<Business> getCompactElementsInPage(HtmlNode targetpagenode)
        {
            IEnumerable<HtmlNode> nodes = targetpagenode.Descendants(0)
             .Where(n => n.HasClass("company") && !n.HasClass("company_ad"));
            foreach (HtmlNode node in nodes)
            {
                Debug.WriteLine(node.InnerHtml);
                var name = getName(node);
                if(name==null)
                {
                    Debug.WriteLine("wrong: name parsing");
                }
                var addr = getAddress(node);
                if (name == null)
                {
                    Debug.WriteLine("wrong: addr parsing");
                }
                var relativeLink = getLink(node);
                if (name == null)
                {
                    Debug.WriteLine("wrong: relativeLink parsing");
                }
                var thumb = getThumb(node);

                yield return new Business() {
                   
                    name = name,
                    //desc = getDesc(node),
                    address = addr,
                    link = @"https://www.businesslist.ph"+ relativeLink,
                    imageUrl = thumb,
                    
                    
                };
                           }
        }

        string getPageTitle(HtmlNode targetpagenode)
        {
            var h1 = targetpagenode.SelectSingleNode("//main//section//h1");
            return h1?.InnerHtml;
        }

        string etPageNumber(HtmlNode targetpagenode)
        {
            return "3";
        }
       

        async public Task RunScraper()
        {

            Debug.WriteLine("RunScraper entered");
            await Task.Delay(30);
            //string rawPage = WebHelper.instance.GetPageTextSync(this.TargetPage);
            string rawPage = downloadOrReadFromTargetPageObject(this.TargetPage);

            //string rawPage = File.ReadAllText(@"F:\epicMyth-tmp-6-2022\freelancing\projects\businesses\List of Companies in Manila, Philippines - Companies in Manila - Page 3.html");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawPage);
            ResolvedTitle = getPageTitle(doc.DocumentNode) ;
            OnResolved?.Invoke(this, ResolvedTitle);
            var compactElements = getCompactElementsInPage(doc.DocumentNode).ToList();
            int i = 0;
            foreach (var item in compactElements)
            {
                
                Console.WriteLine(" resolving element: name: " + item.name);
                resolveFullElement(item);
                i++;
                OnProgress?.Invoke(this, new DownloadingProg() { Total = compactElements.Count, Current = i });
                OnTaskDetail?.Invoke(this, $"downloading company info: {item.name}"); 

            }
            Debug.WriteLine("saving csv");
            //Debug.Write(rawPage);
            string uniqueOutputFileName = Utils.SanitizeFileName(this.ResolvedTitle)+ ".csv";
            var outputPath = Path.Combine(WorkspaceDirectory, ConfigService.Instance.CSVOutputRelativeLocation, uniqueOutputFileName);
            using (var writer = new StreamWriter(outputPath))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(compactElements);
            }

            Debug.WriteLine("saved:" + outputPath);
            return;
            
            Stage = ScrapTaskStage.DownloadingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            



        }
    }
}
