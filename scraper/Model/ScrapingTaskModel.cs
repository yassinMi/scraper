using scraper.Model.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using scraper.Services;

namespace scraper.Model
{

    public enum ScrapTaskStage { Ready, DownloadingData, Paused, ConvertingData, Success, Failed }

  

    public class ScrapingTaskJsonModel
    {
        public ScrapTaskStage Stage ;
        public string OriginalURL;
        public string Title;
        public string OutputCSVFilename;
        public string TargetPageSavedHtmlFilename;

        public string[] DownloadedIDS;
        public DownloadingProg DownloadingProgress;
        public bool IsCompleted;
    }

    public class ScrapingTaskModel 
    {
        public ScrapingTaskModel(string targetPage)
        {
            this.TargetPage = targetPage;
            Stage = ScrapTaskStage.Ready;

        }

        public ScrapingTaskModel()
        {
        }

        public ScrapingTaskModel(ScrapingTaskJsonModel obj)
        {
            this.Title = obj.Title;
            TargetPage = obj.OriginalURL;
            OutputCSVFilename = obj.OutputCSVFilename;
            TargetPageSavedHtmlFilename = obj.TargetPageSavedHtmlFilename;
            DownloadingProgress = obj.DownloadingProgress;
            Stage = obj.Stage;// ? ScrapTaskStage.Success : ScrapTaskStage.Paused;

        }

        public string Title { get; set; }

        public string TargetPage { get; set; }
        public string TargetPageSavedHtmlFilename { get; set; }
        public string OutputCSVFilename { get; set; }


        public DownloadingProg DownloadingProgress { get; set; }
        public ScrapTaskStage Stage { get; set; } = ScrapTaskStage.Ready;


       

        private Process Process { get; set; }




        /// <summary>
        /// fired as the stdout prints progress information
        /// </summary>
        public event EventHandler<DownloadingProg> OnProgress;
        public event EventHandler<string> OnTaskDetail; //stdout prints something like T: downloading image.jpg
        public event EventHandler<string> OnError;  //stderr prints something
        public event EventHandler<Process> OnProcess; //process object constructed
        public event EventHandler<ScrapTaskStage> OnStageChange; //stdout prints something like P: [4/35]
        public event EventHandler<int> OnExitedSuccessfully; //exited with code 0
        public event EventHandler<int> OnExitedWithError; //exited with coe !=0
        public event EventHandler<int> OnExited; //exited with any code
        public event EventHandler OnRunning;    //process started 

        StringBuilder Builder;

        /// <summary>
        /// </summary>
        public async Task<int> RunScraper()
        {
            Builder = new StringBuilder();
            string python_args = "scraper.py " + "\"" + TargetPage + "\"";
            Process = Utils. constructProcess("python.exe", python_args, @"E:\TOOLS\scraper\scraper\scripts");
            OnProcess?.Invoke(this, Process);
            DataReceivedEventHandler hndl = ((sender, args) =>
            {
                if (args.Data == null) return;
                Debug.WriteLine("data in :  " + args.Data);
                Builder.AppendLine(args.Data);
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                if (args.Data.StartsWith("S: "))
                {
                    string stage = args.Data.Substring(3);
                    switch (stage)
                    {
                        case "resolving target page":
                            Stage = ScrapTaskStage.Ready;
                            OnStageChange?.Invoke(this, ScrapTaskStage.DownloadingData); break;
                        case "downloading":
                            Stage = ScrapTaskStage.DownloadingData;
                            OnStageChange?.Invoke(this, ScrapTaskStage.DownloadingData); break;
                        case "done": //NOTE the scraper.py being done only mean that the ScrapTask's doanwloading stage is finished, we still have to execute the converter.py
                                     //Stage = ScrapTaskStage.Success; //this was a mistake
                                     //OnStageChange?.Invoke(this, ScrapTaskStage.Success); break;
                        default:
                            break;
                    }
                }
                else if (args.Data.StartsWith("T: "))
                {
                    OnTaskDetail?.Invoke(this, args.Data.Substring(3));
                }
                else if (args.Data.StartsWith("P: "))
                {
                    var m = Regex.Match(args.Data.Substring(3), "\\[(\\d+)/(\\d+)\\]");
                    if (m.Success)
                    {
                        try
                        {
                            string total = m.Groups[2].Value, current = m.Groups[1].Value;
                            DownloadingProgress = new DownloadingProg() { Total = int.Parse(total), Current = int.Parse(current) };
                            OnProgress?.Invoke(this, DownloadingProgress);
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
                else if (args.Data.StartsWith("I: "))
                {
                    if (args.Data.StartsWith("I: page_file_name:"))
                    {
                        TargetPageSavedHtmlFilename = args.Data.Substring(18);
                    }
                }
            });
            //  Process.OutputDataReceived += hndl;
            Process.OutputDataReceived += hndl;
            Process.ErrorDataReceived += (s, e) =>
            {
                OnError?.Invoke(this, e.Data);
            };
            Debug.WriteLine("starting ffmpeg..");

            await Task.Run(new Action(() =>
            {
                Process.Start();
                Process.BeginErrorReadLine();
                Process.BeginOutputReadLine();
            }), new System.Threading.CancellationToken());
            //MI.Verbose
            OnRunning?.Invoke(this, new EventArgs());
            await Task.Run(new Action(() =>
            {
                Process.WaitForExit();
            }));
            //MI.Verbose(null);
            OnExited?.Invoke(this, Process.ExitCode);
            if ((Process.ExitCode != 0))
            {
                OnExitedWithError?.Invoke(this, Process.ExitCode);
            }
            else
            {
                OnExitedSuccessfully?.Invoke(this, Process.ExitCode);
            }

            return Process.ExitCode;// process.ExitCode;
        }

        internal static ScrapingTaskModel LoadFromFile(string item)
        {
            Debug.WriteLine("LoadFromFile");
            //System.Text.Json k;
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ScrapingTaskJsonModel>(System.IO.File.ReadAllText(item));
                Debug.WriteLine(obj.DownloadingProgress);
                var d = new DownloadingProg();

                Debug.WriteLine(obj.Stage);
                return new ScrapingTaskModel(obj);
            }
            catch (Exception)
            {

                return new ScrapingTaskModel() { Title = "failed"};
            }
           

        }




        /// <summary>
        /// </summary>
        public async Task<int> RunConverter()
        {
            if (string.IsNullOrWhiteSpace(OutputCSVFilename))
            {
                OutputCSVFilename = Path.GetFileName(TargetPageSavedHtmlFilename) + ".csv";
            }
            Trace.Assert(System.IO.File.Exists(Path.Combine(@"E:\TOOLS\scraper\scraper\scripts", TargetPageSavedHtmlFilename)), $"cannot run converter, html page is mising '{TargetPageSavedHtmlFilename}'");
            Builder = new StringBuilder();
            string python_args = "converter.py " + TargetPageSavedHtmlFilename + " " + OutputCSVFilename;
            Process =  Utils.constructProcess("python.exe", python_args, @"E:\TOOLS\scraper\scraper\scripts");
            OnProcess?.Invoke(this, Process);
            DataReceivedEventHandler hndl = ((sender, args) =>
            {
                if (args.Data == null) return;
                Debug.WriteLine("data in :  " + args.Data);
                Builder.AppendLine(args.Data);
                if (string.IsNullOrWhiteSpace(args.Data)) return;



            });
            //  Process.OutputDataReceived += hndl;
            Process.OutputDataReceived += hndl;
            Process.ErrorDataReceived += (s, e) =>
            {
                OnError?.Invoke(this, e.Data);
            };
            Debug.WriteLine("starting converter.py..");

            await Task.Run(new Action(() =>
            {
                Process.Start();
                Process.BeginErrorReadLine();
                Process.BeginOutputReadLine();
            }), new System.Threading.CancellationToken());
            //MI.Verbose
            OnRunning?.Invoke(this, new EventArgs());
            await Task.Run(new Action(() =>
            {
                Process.WaitForExit();
            }));
            //MI.Verbose(null);
            OnExited?.Invoke(this, Process.ExitCode);
            if ((Process.ExitCode != 0))
            {
                OnExitedWithError?.Invoke(this, Process.ExitCode);
                Stage = ScrapTaskStage.Failed;
                OnStageChange?.Invoke(this, Stage);

            }
            else
            {
                Stage = ScrapTaskStage.Failed;
                OnStageChange?.Invoke(this, Stage);
                OnExitedSuccessfully?.Invoke(this, Process.ExitCode);
            }

            return Process.ExitCode;// process.ExitCode;
        }





    }

   
}
