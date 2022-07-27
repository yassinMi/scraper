using BusinesslistPhPlugin.Model;
using CsvHelper;
using HtmlAgilityPack;
using scraper.Core;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BusinesslistPhPlugin
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
                return new ElementDescription()
                {
                    ID = "2",
                    Name = "Business",
                    /*Fields = typeof(Business).GetProperties().Select(p => new Field()
                    {
                        Name = p.Name,
                        
                         NativeType=p.PropertyType,
                         UIName = CoreUtils.CamelCaseToUIText(p.Name)
                         
                    }).Cast<IField>()*/ //the generic default (when switching to base class approach)
                    Fields = new IField[]
                    {
                        new Field() {Name="company", UIName="Company Name", UIHeaderWidth=85 },
                        new Field() {Name="contactPerson", UIName="Contact Person", UIHeaderWidth=80 , UserDescription="Contact person or company manager"},
                        new Field() {Name="address", UIName="Address", UIHeaderWidth=70 },
                        new Field() {Name="phonenumber", UIName="Phone", UIHeaderWidth=80 },
                        new Field() {Name="email", UIName="Email", UIHeaderWidth=70 },
                        new Field() {Name="employees", UIName="Employees", UIHeaderWidth=70 },
                        new Field() {Name="website", UIName="Website", UIHeaderWidth=70 },
                        new Field() {Name="year", UIName="Year", UIHeaderWidth=65 },
                        //new Field() {Name="imageUrl", UIName="Image Url", UIHeaderWidth=70 },
                        new Field() {Name="link", UIName="Link", UIHeaderWidth=70 },
                        new Field() {Name="description", UIName="Description", UIHeaderWidth=70 },

                    }
                };
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

        public PluginUsageInfo UsageInfo
        {
            get
            {
                return new PluginUsageInfo()
                {
                    UsageInfoViewHeader = "Supported URL's:",
                    UseCases = new TargetPageUrlUseCaseHelp[]
                    {
                        new TargetPageUrlUseCaseHelp() {Description="Locations pages",
                        ExampleUrls = new string[]
                        {
                            "https://www.businesslist.ph/location/santa-rosa-city/5"
                        }
                        },
                        new TargetPageUrlUseCaseHelp() {Description="Categories pages",
                        ExampleUrls = new string[]
                        {
                            "https://www.businesslist.ph/category/industrial-premises",
                        }
                        },
                        /*new TargetPageUrlUseCaseHelp() {Description="Other pages",
                        ExampleUrls = new string[]
                        {
                            "search results pages etc",
                        }
                        },*/

                    }
                };
            }
        }

        public Type ElementModelType
        {
            get
            {
                return typeof(Business);
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

        public TaskStatsInfo TaskStatsInfo { get; set; } = new TaskStatsInfo();


        public string ResolvedTitle { get; set; } = null;

        public ScrapTaskStage Stage { get; set; }

        public string TargetPage { get; set; } = null;

        public event EventHandler<string> OnError;
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnResolved;
        public event EventHandler<string> OnPage;
        public event EventHandler<ScrapTaskStage> OnStageChanged;
        public event EventHandler<string> OnTaskDetail;

        public void Pause()
        {
            throw new NotImplementedException();
        }

        

        //not used
        async public Task RunConverter()
        {
            Stage = ScrapTaskStage.ConvertingData;
            this.OnStageChanged?.Invoke(this, this.Stage);
            await Task.Delay(130);
            Stage = ScrapTaskStage.Success;
            this.OnStageChanged?.Invoke(this, this.Stage);

        }

        /// <summary>
        /// tuple: key , formatted value (may not be good), original value node (use it for custom formatting), bool isSpan
        /// </summary>
       public static IEnumerable<Tuple<string,string,HtmlNode,bool>> getInfos(HtmlNode elementPage)
        {
            foreach (var infoElem in elementPage.SelectNodes("//div[@class='info']"))
            {
                var labelElem = infoElem.SelectSingleNode("./*[@class='label']");
                if (labelElem == null) continue;
                int cc = infoElem.ChildNodes.Count;
                string key = labelElem.InnerText;
                string Formattedvalue = "";
                HtmlNode originalValueNode = null; ;
                bool IsSpan = false;
                if (labelElem.OriginalName == "span"){
                    
                    var firstTextNode = infoElem.SelectSingleNode("./text()");
                    if (firstTextNode == null) continue;
                    Formattedvalue = firstTextNode.InnerText;
                    originalValueNode = firstTextNode;
                    IsSpan = true;
                }
                else if(labelElem.OriginalName == "div")
                {
                    
                    if (cc >=2)
                    {
                        originalValueNode = infoElem.ChildNodes[1];
                        Formattedvalue = CoreUtils.StripHTML(originalValueNode.InnerHtml);
                    }
                    else if (cc == 1)
                    {
                        Debug.WriteLine("cc=1: " + infoElem.FirstChild.InnerHtml);
                        continue;
                    }
                    
                }
                yield return new Tuple<string, string, HtmlNode, bool>(key, Formattedvalue, originalValueNode,IsSpan);

            }
        }
       public static string getPhoneNumber(HtmlNode elementPage)
        {
            return elementPage.SelectSingleNode("//div[@class='text phone']")?.InnerText;
        }
        public static string getEmail(HtmlNode elementPage)
        {
            //System.Net.WebUtility.HtmlDecode
            string email = elementPage.InnerHtml;
            Regex regex = new Regex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)");
            Match match = regex.Match(email);
            if (match.Success)
                return match.Value;
            else
                return "N/A";
        }
        public static string getWebsite(HtmlNode elementPage)
        {
            return "unr";
        }
       public static string getEmployees(HtmlNode elementPage)
        {
            return "unr";
        }

        static public HtmlNode getget()
        {
            return new HtmlNode(HtmlNodeType.Element, new HtmlDocument(),5);
        }

       

        /// <summary>
        /// exceptions: HttpRequestException
        /// link is hashed and used as filename (+.html) 
        /// if the file exists under the specified folder: reads and returns it's content as string, no web request
        /// else: fetch and save object under the specified folder, returning the same saved html string
        /// </summary>
        /// <param name="businessLink"></param>
        public static string downloadOrRead(string pageLink, string folder)
        {
            Debug.WriteLine("downloadOrRead ");
            string uniqueFilename = Path.Combine(folder,CoreUtils. getUniqueLinkHash(pageLink) + ".html")  ;
            if (File.Exists(uniqueFilename)){return File.ReadAllText(uniqueFilename);}
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
        /// excpections: HttpRequestException
        /// the missing fields are:  phone email website, employees
        /// existing fields are  name, desc, thumb, location, link
        /// </summary>
        /// <param name="compactElement"></param>
        public static void resolveFullElement( Business compactElement, out int writtenBytes , out int WrittenObjectsCoutnt)
        {
            writtenBytes = 0; WrittenObjectsCoutnt = 0;

            Debug.WriteLine("downloading or reading html link: "+ compactElement.link);
            
            string rawElementPage = downloadOrRead(compactElement.link, Workspace.Current.HtmlObjectsFolder);
            WrittenObjectsCoutnt++;
            writtenBytes += rawElementPage.Length;
            HtmlDocument elementDoc = new HtmlDocument();
            elementDoc.LoadHtml(rawElementPage);
            Debug.WriteLine("resolving missing fields from html");

            //field parsing buffers
            List<string> phones = new List<string>(8);
            string[] persons = new string[2] { null, null };
            foreach (var infoItem in getInfos(elementDoc.DocumentNode))
            {
                /* the info items keys:
Company name
Address
Phone Number
Website
Location map
Description
Working hours
Listed in categories
Keywords
Mobile phone
Contact Person
Establishment year
Employees
E-mail
Products & Services
Fax
Company manager
Registration code
School name
Hotel name
VAT registration
Restaurant name
College name
University name
Hospital name
Video


Video
*/

                //phone

                if (infoItem.Item1== "Phone Number" || infoItem.Item1 == "Mobile phone")
                {
                    foreach (var item in infoItem.Item3.SelectNodes(".//text()"))
                    {
                        phones.Add(item.InnerText.Trim());
                    }
                }


                //email

                //website
                else if (infoItem.Item1 == "Website")
                {
                    compactElement.website = infoItem.Item2;
                }
                else if (infoItem.Item1 == "Employees" && infoItem.Item4)
                {
                    compactElement.employees = infoItem.Item2?.Trim();
                }
                else if (infoItem.Item1 == "Establishment year" && infoItem.Item4)
                {

                    compactElement.year = infoItem.Item2?.Trim();
                }
                else if (infoItem.Item1 == "Contact Person" )
                {
                    persons[0] = $"{infoItem.Item2?.Trim()}(Contact Person)";
                }
                else if (infoItem.Item1 == "Company manager")
                {
                    persons[1] = $"{infoItem.Item2?.Trim()}(Company Manager)";
                }

                //employees

            }
            
            
            compactElement.contactPerson = string.Join(", ", persons.Where(s=>!string.IsNullOrWhiteSpace(s)));


            compactElement.phonenumber = string.Join(" / ", phones);
            //compactElement.email = "N/A"; //not available
            //old parsers
            //compactElement.phonenumber = getPhoneNumber(elementDoc.DocumentNode);
            compactElement.email = getEmail(elementDoc.DocumentNode);
            //compactElement.employees = getEmployees(elementDoc.DocumentNode);
            Debug.WriteLine("done");
        }

        
        public static string ClearPageNumFromUrl(string url)
        {
            return Regex.Replace(url, @"/\d+$|/\d+/$", "");
        }

        /// <summary>
        /// ulr must be cleared with ClearPageNumFromUrl
        /// </summary>
        public static string AppendPageNumToUrl(string url,int pageNum)
        {
            return url.TrimEnd(new char[] { '/' }) + "/" + pageNum.ToString();
        }

        public static string getAddress(HtmlNode node)
        {
            var div = node.SelectSingleNode(".//div[@class='address']");
            if (div == null) return null;
            if  (string.IsNullOrWhiteSpace(div.InnerHtml) ) return null;
            return System.Net.WebUtility.HtmlDecode(CoreUtils. StripHTML(div.InnerHtml));
        }
        public static string getName(HtmlNode node)
        {
            return System.Net.WebUtility.HtmlDecode( node.SelectSingleNode("./h4//a")?.InnerText);
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

        /// <summary>
        /// for targte pages loaction and cats
        /// assumes the last page is there (aka not a one page other non suitable inputs)
        /// </summary>
        public static int getLastPageNumber(HtmlNode pagenode)
        {
            var lasPafeNo = pagenode.SelectNodes("//a[@class='pages_no']").LastOrDefault();
            if (lasPafeNo == null) throw new Exception("error while paring getLastPageNumber");
            int res = int.Parse(lasPafeNo.InnerText);
            if(res <2) throw new Exception("getLastPageNumber: number cannot be <2");
            return res;
        }

        /// <summary>
        /// tested
        /// </summary>
        public static bool isBusinessesListings(HtmlNode pagenode)
        {
            bool res = true;
            res &= pagenode.SelectSingleNode("//div[@id='listings']") != null;
            res &= pagenode.SelectSingleNode("//div[@id='listings']") != null;
            return res;
        }
        /// <summary>
        /// tested
        /// </summary>
        public static bool isNoneEmptyBusinessesListings(HtmlNode pagenode)
        {
            bool res = true;
            res &= isBusinessesListings(pagenode);
            res &= pagenode.SelectSingleNode("//div[@class='pages_container_top']") != null;
            return res;
        }

        /// <summary>
        /// tested
        /// </summary>
        public static bool isMultiplePagesBusinessesListings(HtmlNode pagenode)
        {
            bool res = true;
            res &= isBusinessesListings(pagenode) && isNoneEmptyBusinessesListings(pagenode);
            res &= pagenode.SelectSingleNode("//div[@class='pages_container']") != null;
            return res;
        }

        /// <summary>
        /// tuple: curr page, total pages, curr page node
        /// </summary>
        /// <param name="targetRootPage">ignores page numbers it always start from root </param>
        /// <returns></returns>
        public static IEnumerable<Tuple<int,int,HtmlNode>> EnumeratePages(string targetRootPage)
        {
            string pageRaw= downloadOrRead(targetRootPage, Workspace.Current.TPFolder);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageRaw);
            var paegs_container = doc.DocumentNode.SelectSingleNode("//div[@class='pages_container']");
            
            if((isBusinessesListings(doc.DocumentNode)==false) || (isNoneEmptyBusinessesListings(doc.DocumentNode) == false))
            {
                yield break;
            }
           
            int min = 1;
            int max = 1;
            if (isMultiplePagesBusinessesListings(doc.DocumentNode))
            {
                max = getLastPageNumber(doc.DocumentNode);
            }
            foreach (var pg in Enumerable.Range(min, max))
            {
                string pageLink = AppendPageNumToUrl(ClearPageNumFromUrl(targetRootPage), pg);
                string raw = downloadOrRead(pageLink, Workspace.Current.TPFolder);
                HtmlDocument newDoc = new HtmlDocument();
                newDoc.LoadHtml(raw);
                yield return new Tuple<int,int, HtmlNode>(pg, max, newDoc.DocumentNode);
            }

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
                   
                    company = name,
                    //desc = getDesc(node),
                    address = addr,
                    link = @"https://www.businesslist.ph"+ relativeLink,
                    imageUrl = thumb,
                    
                    
                };
                           }
        }

        public static string getPageTitle(HtmlNode targetpagenode)
        {
            var h1 = targetpagenode.SelectSingleNode("//main//section//h1");
            return h1?.InnerHtml;
        }

        public static string etPageNumber(HtmlNode targetpagenode)
        {
            return "3";
        }

        static Synchronizer<string> targetPageBasedLock = new Synchronizer<string>();

        /// <summary>
        /// static deps: Workspace.current,
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        async public Task RunScraper(CancellationToken ct)
        {
            lock (targetPageBasedLock[TargetPage])
            {
                Debug.WriteLine("RunScraper entered");
                TaskStatsInfo.Reset();
                Stage = ScrapTaskStage.DownloadingData;
                this.OnStageChanged?.Invoke(this, this.Stage);
                string raw;
                try
                {
                    raw = downloadOrRead(TargetPage, Workspace.Current.TPFolder);
                    TaskStatsInfo.incSize(raw.Length);
                }
                catch(HttpRequestException )
                {
                    Stage = ScrapTaskStage.Failed;
                    this.OnStageChanged?.Invoke(this, this.Stage);
                    OnError?.Invoke(this, "Network error");
                    return;
                }
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(raw);
                if ((isBusinessesListings(doc.DocumentNode)) == false || (isNoneEmptyBusinessesListings(doc.DocumentNode) == false))
                {
                    return;
                }
                ResolvedTitle = getPageTitle(doc.DocumentNode); ///+ ;
                OnResolved?.Invoke(this, ResolvedTitle);
                try
                {

                    string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle) + ".csv";
                    var outputPath = Path.Combine(Workspace.Current.CSVOutputFolder, uniqueOutputFileName);

                    foreach (var page in EnumeratePages(TargetPage))
                {
                    OnPage?.Invoke(this, $"[page {page.Item1}/{page.Item2}]");
                    var compactElements = getCompactElementsInPage(page.Item3).ToList();
                        List<Business> resolvedElements = new List<Business>(compactElements.Count);
                    int i = 0;
                    foreach (var item in compactElements)
                    {
                            if (ct.IsCancellationRequested)
                            {
                                Debug.WriteLine("saving csv");
                                CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                                Debug.WriteLine("saved current page conent:" + outputPath);
                                Stage = ScrapTaskStage.Success;
                                this.OnStageChanged?.Invoke(this, this.Stage);
                                return;
                            }
                        int objs, bytes = 0;
                        resolveFullElement(item, out bytes, out objs);
                            resolvedElements.Add(item);
                            TaskStatsInfo.incObject(objs);
                            TaskStatsInfo.incSize(bytes);
                            TaskStatsInfo.incElem(1);
                        i++;
                        OnProgress?.Invoke(this, new DownloadingProg() { Total = compactElements.Count, Current = i });
                        OnTaskDetail?.Invoke(this, $"Collecting business info: {item.company}");
                    }
                    Debug.WriteLine("saving csv");
                    CSVUtils.CSVWriteRecords(outputPath, resolvedElements, page.Item1 > 1);
                    Debug.WriteLine("saved current page conent:" + outputPath);
                }

                    Stage = ScrapTaskStage.Success;
                    this.OnStageChanged?.Invoke(this, this.Stage);
                    return;

                }
                catch 
                {
                    Debug.WriteLine("catched");
                    Stage = ScrapTaskStage.Failed;
                    this.OnStageChanged?.Invoke(this, this.Stage);
                    OnError?.Invoke(this, "Network Error");
                    return;
                }

                return;

                Stage = ScrapTaskStage.DownloadingData;
                this.OnStageChanged?.Invoke(this, this.Stage);

            }




        }
    }
}
