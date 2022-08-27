using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// UI helper types such as Prompt objects, 
/// </summary>
namespace scraper.Core.UI
{

    public enum PromptType { Error, Warning, Information,
        Question
    }
    /// <summary>
    /// describes the required information to show a dlg window or message box on the scrpare assembly.
    /// this allows invoking prompts from core or plugin assemblies independent of the imlementation details at scraper.exe
    /// </summary>
    public class PromptContent
    {
        public PromptContent()
        {
            //for design data
        }
        public PromptContent(string message, string title, string [] buttons, PromptType type)
        {
            Type = type;
            Message = message;
            Buttons = buttons;
            Title = title;
        }
        /// <summary>
        /// information , OK, title= "scraper"
        /// </summary>
        /// <param name="message"></param>
        public PromptContent(string message)
        {
            Message = message;
            Type = PromptType.Information; Title = "Scraper";
            Buttons = new string[] { "OK" };
        }
        public PromptType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string[] Buttons { get; set; }

    }


    public class PromptRequestEventArgs
    {
        public PromptRequestEventArgs(PromptContent p, Action<string> hndl)
        {
            this.PromptContent = p; this.PromptResponseHandler = hndl;
        }
        public PromptContent PromptContent { get; set; }
        public Action<string> PromptResponseHandler { get; set; }
    }
}
