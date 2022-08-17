using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using scraper.Core;
using scraper.Core.Attributes;
using scraper.Core.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using scraper.Core.Workspace;
using System.Diagnostics;

[assembly:CoreAPIVersion("0.1.2")]
namespace DNTestPlugin
{
    /// <summary>
    /// todo extract generic class for dynamic scraping with se from this plugin
    /// </summary>
    public class NpmScrapingTask : ScrapingTaskBase
    {
        public NpmScrapingTask(string tp)
        {
            TargetPage = tp;
        }
        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override Task RunConverter()
        {
            throw new NotImplementedException();
        }
        static Synchronizer<string> _lock = new Synchronizer<string>();
        static WebDriver mainWebDriver { get; set; } = null;
        const string CAT_CLASS = "kl";
        const string TOPIC_CLASS = "css-l3rx45";


        public override async Task RunScraper(CancellationToken ct)
        {
            Debug.WriteLine("RunScraper call..");

            lock (_lock[TargetPage])
            {
                List<Topic> list = new List<Topic>();
                if (Workspace.Current?.CSVOutputFolder == null) Debug.WriteLine("null ws");
                ActualOutputFile = Path.Combine(Workspace.Current.CSVOutputFolder, "All topics"+".csv");
                IWebElement categories_welements_wrapper;
                OnTaskDetailChanged("Waiting for other task(s) to end..");
                lock (_lock)//concurrent tasks can'y run this part of code simultaneously
                {

                    if (mainWebDriver == null)
                    {
                        OnStageChanged(ScrapTaskStage.Setup);
                        OnTaskDetailChanged("Starting chrome driver..");
                        mainWebDriver = new ChromeDriver();
                    }
                        
                    mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                    OnBrowserWindowsCountChanged(++BrowserWindowsCount);
                    Debug.WriteLine("GoToUrl driver..");
                    mainWebDriver.Url = TargetPage;
                    OnStageChanged(ScrapTaskStage.DownloadingData);
                    try
                    {

                    
                    mainWebDriver.Navigate();
                    OpenQA.Selenium.Support.UI.WebDriverWait w = new OpenQA.Selenium.Support.UI.WebDriverWait(mainWebDriver, TimeSpan.FromSeconds(30));
                        
                        w.Until((e) =>
                    {
                        try
                        {
                            return e.FindElement(By.ClassName("Sidebar__Container-gs0c67-0")) != null;
                        }
                        catch (StaleElementReferenceException err)
                        {
                            throw;
                        }
                    });

                    OnResolved("NPM Docs topics");
                    Debug.WriteLine("getting categories_welements_wrapper..");
                    categories_welements_wrapper = mainWebDriver.FindElement(
                        By.ClassName("Sidebar__Container-gs0c67-0"));
                    }
                    catch (Exception err)
                    {

                        OnError(err.Message);
                        OnStageChanged(ScrapTaskStage.Failed);
                        return;
                    }

                    // By.XPath("/*[@class='Sidebar__Container-gs0c67-0 bXQeSB sidebar']"));
                    Debug.WriteLine("enumerating categories_welements..");
                var categories_welements = categories_welements_wrapper.FindElements(By.XPath("./div"));
                foreach (var cat in categories_welements)
                {
                    
                    string current_cat = getCategoryFromCatElem(cat);
                    OnPageStarted(current_cat);
                    Debug.WriteLine($"enumerating topics in {current_cat}..");
                    var topics_welements = cat.FindElements(By.XPath($".//div[@class='{TOPIC_CLASS}']"));
                    int i = 0;
                    foreach (var item in topics_welements)
                    {
                        i++;
                        Topic new_cmp_topic = new Topic();
                        Debug.WriteLine($"creating new comp_elem ..");
                        new_cmp_topic.Title = getTitle(item);
                        new_cmp_topic.Category = current_cat;
                        new_cmp_topic.SubTitle = getSubTitle(item);
                        new_cmp_topic.Link = getLink(item);
                        TaskStatsInfo.incElem(1);
                        Debug.WriteLine($"delaying ..");
                        OnStageChanged(ScrapTaskStage.Delaying);
                        Task.Delay(500).GetAwaiter().GetResult();
                        list.Add(new_cmp_topic);
                        OnProgress(new DownloadingProg() { Current = i, Total = topics_welements.Count });
                    }
                }
                }

                CSVUtils.CSVWriteRecords(ActualOutputFile, list, false);
                OnStageChanged(ScrapTaskStage.Success);
                return;
                
            }
        }

        private string getLink(IWebElement item)
        {
            //the first child a 
            var a = item.FindElement(By.XPath("./a"));
            return a.GetAttribute("href");
        }

        private string getSubTitle(IWebElement item)
        {
            //the first child a > first span
            var s = item.FindElement(By.XPath("./a/span"));
            return s.Text;
        }

        private string getCategoryFromCatElem(IWebElement cat)
        {
            //re first button children
            var butt = cat.FindElement(By.XPath("./button"));
            return butt.Text;
        }

        private string getTitle(IWebElement item)
        {
            //the first child a 
            var a = item.FindElement(By.XPath("./a"));
            return a.Text;
        }
    }
}
