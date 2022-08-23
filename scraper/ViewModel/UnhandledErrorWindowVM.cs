using Mi.Common;
using scraper.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace scraper.ViewModel
{
    public class UnhandledErrorWindowVM : BaseViewModel
    {




        private Exception _ExceptionObj;
        public Exception ExceptionObj
        {
            set { _ExceptionObj = value; notif(nameof(ExceptionObj));
                ExceptionDetailsText = ExceptionObj?.ToString();
            }
            get { return _ExceptionObj; }
        }


        public UnhandledErrorWindow WindowObj { get; set; }



        private string _ExceptionDetailsText;
        public string ExceptionDetailsText
        {
            set { _ExceptionDetailsText = value; notif(nameof(ExceptionDetailsText)); }
            get { return _ExceptionDetailsText; }
        }



        public enum UnhandledErrorWindowDialogResult {none=0, restart, close };

        private bool _IsDetailsExpanded;
        public bool IsDetailsExpanded
        {
            set { _IsDetailsExpanded = value; notif(nameof(IsDetailsExpanded)); }
            get { return _IsDetailsExpanded; }
        }


        private UnhandledErrorWindowDialogResult _DialogChosedAction = UnhandledErrorWindowDialogResult.none;
        public UnhandledErrorWindowDialogResult DialogChosedAction
        {
            set { _DialogChosedAction = value; notif(nameof(DialogChosedAction)); }
            get { return _DialogChosedAction; }
        }



        public ICommand CloseCommand { get { return new MICommand(hndlCloseCommand); } }

        private void hndlCloseCommand()
        {
            DialogChosedAction = UnhandledErrorWindowDialogResult.close;
            WindowObj.Close();

        }

        public ICommand RestartCommand { get { return new MICommand(hndlRestartCommand); } }

        private void hndlRestartCommand()
        {
            DialogChosedAction = UnhandledErrorWindowDialogResult.restart;
            WindowObj.Close();
        }

        public ICommand ExpandDetailsCommand { get { return new MICommand(hndlExpandDetailsCommand); } }

        private void hndlExpandDetailsCommand()
        {
            IsDetailsExpanded = true;
        }

        public ICommand CollapseDetailsCommand { get { return new MICommand(hndlCollapseDetailsCommand); } }

        private void hndlCollapseDetailsCommand()
        {
            IsDetailsExpanded = false;
        }



        string getDetailsText()
        {
            return ExceptionObj.ToString();
            var exp = ExceptionObj;
            StringBuilder sb = new StringBuilder();
            do
                sb.AppendLine(exp.Message);
            while ((exp = exp.InnerException) != null);
            Debug.WriteLine("res:"+sb.ToString());
            
            return sb.ToString();
            //return "copying error details is not implemented yet";
        }

        public ICommand CopyDetailsCommand { get { return new MICommand(hndlCopyDetailsCommand); } }

        private void hndlCopyDetailsCommand()
        {
            try
            {
                Clipboard.SetDataObject(getDetailsText());
            }
            catch (Exception err)
            {
                Core.Utils.CoreUtils.WriteLine($"hndlCopyDetailsCommand: unkow exception [gnored][{DateTime.Now}] : {Environment.NewLine}{err}");
            }
        }
    }
}
