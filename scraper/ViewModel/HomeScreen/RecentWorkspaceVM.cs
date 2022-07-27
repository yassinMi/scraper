using Mi.Common;
using scraper.Model;
using scraper.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace scraper.ViewModel.HomeScreen
{
    public class RecentWorkspaceVM : BaseViewModel
    {

        public MainViewModel mainViewModelRef { get; set; }

        private string _TotalSize;
        public string TotalSize
        {
            set { _TotalSize = value; notif(nameof(TotalSize)); }
            get { return Utils.BytesToString(Directory. CreateDirectory(model.Path). GetFiles("*", SearchOption.AllDirectories).Sum(f=>f.Length)); }
        }


        private string _Name;
        public string Name
        {
            set { _Name = value; notif(nameof(Name)); }
            get { return  Path.GetFileName(model.Path); }
        }


        private bool _IsPinned;
        public RecentWorkspace model { get; set; }

        public RecentWorkspaceVM(RecentWorkspace p)
        {
            this.model = p;
            this.IsPinned = p.IsPinned;
        }

        public bool IsPinned
        {
            set { _IsPinned = value; notif(nameof(IsPinned)); }
            get { return _IsPinned; }
        }

        public ICommand OpenCommand { get { return new MICommand(hndlOpenCommand); } }


        private void hndlOpenCommand()
        {
            mainViewModelRef.OpenRecentWorkspaceCommand.Execute(this.model.Path);
        }


        public ICommand PinToggleCommand { get { return new MICommand(hndlPinToggleCommand); } }

        private void hndlPinToggleCommand()
        {
            mainViewModelRef.RecentWorkspacePinToggleCommand.Execute(this.model);
        }


        public ICommand RemoveCommand { get { return new MICommand(hndlRemoveCommand); } }

        private void hndlRemoveCommand()
        {
            throw new NotImplementedException();
        }



    }
}
