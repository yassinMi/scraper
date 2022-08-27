using Mi.Common;
using scraper.Core.UI;
using scraper.View; //todo fix mvvm
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace scraper.ViewModel
{
    public class PromptWindowVM:BaseViewModel
    {
        public PromptWindowVM()
        {
            //design data
        }

        public PromptWindowVM(PromptContent content)
        {
            PromptContent = content;
            Buttons = content.Buttons;
            notif(nameof(Buttons));
        }
        private string[] _buttons;
        public string[] Buttons
        {
            set { _buttons = value; }
            get { return PromptContent.Buttons; }
        }


        public string Result { get; set; } 


        private  scraper.Core.UI.PromptContent _PromptContent ;
        public scraper.Core.UI.PromptContent PromptContent
        {
            set { _PromptContent = value; }//todo remove, temporary for design data
            get { return _PromptContent; }
        }


        public ICommand ClickButtonCommand { get { return new MICommand<string>(hndlClickButtonCommand, canExecuteClickButtonCommand); } }

        private bool canExecuteClickButtonCommand(string arg)
        {
            return true;
        }

        private void hndlClickButtonCommand(string arg)
        {
            Result = arg;
            WindowObj?.Close();
        }



        public PromptWindow WindowObj { get; set; }
    }
}
