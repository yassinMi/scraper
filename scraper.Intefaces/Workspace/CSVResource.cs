using scraper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Workspace
{
    public class CSVResource
    {
        public Uri Path { get; set; }
        public int Rows { get; set; }
        public bool isChecked { get; set; } = false;
        public bool isRemoved { get; set; }
        public bool isbadFormat { get; set; }
        public void Check(Plugin plugin)
        {
            isChecked = true;
            if (!File.Exists(Path.OriginalString)) { isRemoved = true; return; }
            isRemoved = false;
            int total_rows = 0;
            int valid_rows = 0;
            isbadFormat = !CSVUtils.checkCSV(Path.OriginalString, plugin.ElementDescription,  out total_rows, out valid_rows);
            Debug.WriteLine("cheking returned: cont " + total_rows);
            Debug.WriteLine("cheking returned: valid " + valid_rows);


            Rows = valid_rows;
        }
    }
}
