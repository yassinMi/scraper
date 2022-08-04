using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using scraper.Core;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleSearchPlugin
{
    public class GSScraper: Plugin
    {
        public override ElementDescription ElementDescription
        {
            get
            {
                return new ElementDescription()
                {
                    Name = "google search result",
                    Fields = new Field[]
                {
                    new Field() {
                        Name = nameof(Model.GoogleResult.title),
                        NativeType = typeof(string), IsRequired=true
                    },
                    new Field() {
                        Name = nameof(Model.GoogleResult.url),
                        NativeType = typeof(double), IsRequired=true
                    },
                   
                },
                    ID = "google search result"
                };
            }
        }

        public override bool ValidateTargetPageInputQuery(string input)
        {
            return true;
        }
        public override Type ElementModelType
        {
            get
            {
                return typeof(Model.GoogleResult);
            }
        }

        public override string ElementName
        {
            get
            {
                return "Google result";
            }
        }

        public override string ElementNamePlural
        {
            get
            {
                return "Google results";
            }
        }

        public override string Name
        {
            get
            {
                return "Google search scraper";
            }
        }





        public override PluginScrapingTask GetTask(TaskInfo taskInfo)
        {
            return new GSScraperScrapingTask();
        }

        public override PluginScrapingTask GetTask(string targetPage)
        {
            return new GSScraperScrapingTask() { TargetPage = targetPage };
        }
        public override PluginUsageInfo UsageInfo
        {
            get
            {
                return new PluginUsageInfo()
                {
                    UsageInfoViewHeader = "Copy & paste google results url",
                    UseCases = new TargetPageUrlUseCaseHelp[]
                     {
                         new TargetPageUrlUseCaseHelp()
                         {
                              Description="google results url:",
                              ExampleUrls = new string[]
                              {
                                  "https://www.google.com/search?q=best+software+testing+tools",
                              }
                         }
                     }
                };
            }
        }

        public override string TargetHost
        {
            get
            {
                return "www.google.com";
            }
        }

    }










    public class GSScraperScrapingTask : PluginScrapingTask
    {






        public override void Pause()
        {

        }
        public override async Task RunConverter()
        {


        }

        static bool tryInitWebDriver()
        {
            try
            {
                if (mainWebDriver != null)
                {
                    //mainWebDriver.Close();
                    mainWebDriver.SwitchTo().NewWindow(WindowType.Tab);
                    return true;
                }
                var opts = new ChromeOptions() { };
                //opts.AddArguments("headless","disable-gpu","no-sandbox");
                var chromdriverservice = ChromeDriverService.CreateDefaultService();
                chromdriverservice.HideCommandPromptWindow = true;
                WebDriver wd = new ChromeDriver(chromdriverservice,opts);
                //wd.Url = @"https://www.google.com/search?q=best+software+testing+tools&rlz=1C1CHWL_enMA1011MA1011&oq=best+software+testing+tools&aqs=chrome..69i57.56629j0j7&sourceid=chrome&ie=UTF-8";
                Debug.WriteLine("initialized wd");

                mainWebDriver = wd;
                return true;
            }
            catch (Exception)
            {

                return false;
            }
            
        }
        object _lock = new object();
        static WebDriver mainWebDriver { get; set; } = null;

        public override async Task RunScraper(CancellationToken ct)
        {
            lock (_lock)
            {
                Stage = ScrapTaskStage.Setup;
                OnStageChanged(Stage);
                TaskDetail = "Starting chrome..";
                OnTaskDetailChanged(TaskDetail);

            if (!tryInitWebDriver())
            {
                Stage = ScrapTaskStage.Failed;
                OnStageChanged(ScrapTaskStage.Failed);
                OnError("failed to start WebDriver");
                return;
            }

            try
            {
                TaskDetail = "Resolving target page..";
                OnTaskDetailChanged(TaskDetail);
                Uri u = new Uri(TargetPage);
                int pages = 1;
                string title = "unknown title";
                int delayms = 200;
                //mainWebDriver.SwitchTo().
                mainWebDriver.Navigate().GoToUrl(TargetPage);
                var basicWrapers = mainWebDriver.FindElements(By.XPath("//div[@class='jtfYYd UK95Uc']"));

                ResolvedTitle = $"{mainWebDriver.Title}";
                OnResolved(ResolvedTitle);
                Stage = ScrapTaskStage.DownloadingData;
                OnStageChanged(Stage);
                string uniqueOutputFileName = CoreUtils.SanitizeFileName(this.ResolvedTitle) + ".csv";
                var outputPath = Path.Combine(Workspace.Current.CSVOutputFolder, uniqueOutputFileName);
                foreach (var item in Enumerable.Range(0, pages))
                {
                    List<Model.GoogleResult> elements_in_page = new List<Model.GoogleResult>();
                    OnPageStarted($"[page {item + 1}/{pages}]");
                    for (int i = 0; i < basicWrapers.Count; i++)
                    {
                        var href_elem = basicWrapers[i].FindElement(RelativeBy.TagName("a"));
                        var firt_h3 = basicWrapers[i].FindElement(RelativeBy.TagName("h3"));
                        if ((href_elem == null) || (firt_h3 == null))
                        {
                            //skip
                            continue;
                        }
                        string result_url = href_elem.GetAttribute("href");
                        string result_title = firt_h3.Text;
                        Model.GoogleResult r = new Model.GoogleResult() { title = result_title, url = result_url };
                        elements_in_page.Add(r);
                        Task.Delay(delayms).GetAwaiter().GetResult();
                        OnProgress(new DownloadingProg() { Total = basicWrapers.Count, Current = i });
                        OnTaskDetailChanged($"Result: {result_title}");
                        this.TaskStatsInfo.incObject(0, 0);
                        TaskStatsInfo.incElem(1);
                    }
                    CSVUtils.CSVWriteRecords(outputPath, elements_in_page, item > 0);

                }
                OnTaskDetailChanged(null);
                Stage = ScrapTaskStage.Success;
                OnStageChanged(Stage);


            }
            catch (Exception err)
            {

                Stage = ScrapTaskStage.Failed;
                OnStageChanged(Stage);
                OnError(err.Message);
                Debug.WriteLine(err.ToString());
                return;
            }



            }
        }
    }
}
