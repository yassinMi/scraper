using scraper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.ViewModel
{
    public class CSVResourceVM : BaseViewModel
    {
        public CSVResourceVM(CSVResource cr)
        {
            if (cr.isChecked == false) cr.Check();
            RowsCount = cr.Rows;
            FilenameOnly = System.IO.Path.GetFileName(cr.Path.OriginalString);
            IsBadFormat = cr.isbadFormat;
            IsRemoved = cr.isRemoved;
            FullPath = cr.Path.OriginalString;
        }
        public CSVResourceVM()
        {
            //d-time
            RowsCount = 455;
            FilenameOnly = "myAwesomeCSV.CSV";
            IsBadFormat = false;
            IsRemoved = false;
            FullPath = "path/to/myAwesomeCSV.CSV";
        }


        private bool _IsRemoved;
        public bool IsRemoved
        {
            set { _IsRemoved = value; notif(nameof(IsRemoved)); }
            get { return _IsRemoved; }
        }

        private bool _IsActive;
        public bool IsActive
        {
            set { _IsActive = value; notif(nameof(IsActive)); }
            get { return _IsActive; }
        }


        private bool _IsBadFormat;
        public bool IsBadFormat
        {
            set { _IsBadFormat = value; notif(nameof(IsBadFormat)); }
            get { return _IsBadFormat; }
        }



        private int _RowsCount;
        public int RowsCount
        {
            set { _RowsCount = value; notif(nameof(RowsCount)); }
            get { return _RowsCount; }
        }


        private string _FilenameOnly;
        public string FilenameOnly
        {
            set { _FilenameOnly = value; notif(nameof(FilenameOnly)); }
            get { return _FilenameOnly; }
        }


        private string _FullPath;
        public string FullPath
        {
            set { _FullPath = value; notif(nameof(FullPath)); }
            get { return _FullPath; }
        }

        internal void recheck()
        {
            IsRemoved = File.Exists(FullPath) == false;
        }
    }
}
