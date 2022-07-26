using scraper.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.ViewModel.HomeScreen
{
    public class RecentWorkspaceVM : BaseViewModel
    {

        private string _TotalSize;
        public string TotalSize
        {
            set { _TotalSize = value; notif(nameof(TotalSize)); }
            get { return Utils.BytesToString(Directory. CreateDirectory(path). GetFiles("*", SearchOption.AllDirectories).Sum(f=>f.Length)); }
        }


        private string _Name;
        public string Name
        {
            set { _Name = value; notif(nameof(Name)); }
            get { return  Path.GetFileName(path); }
        }


        private bool _IsPinned;
        private string path;

        public RecentWorkspaceVM(string p)
        {
            this.path = p;
        }

        public bool IsPinned
        {
            set { _IsPinned = value; notif(nameof(IsPinned)); }
            get { return _IsPinned; }
        }


    }
}
