using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{


    public class TaskStatsInfo
    {
        /// <summary>
        /// number of objects written locally since the start of the task
        /// </summary>
        public int Objects { get; set; }
        /// <summary>
        /// all treated ellements since the start of the task
        /// </summary>
        public int Elements { get; set; }
        /// <summary>
        /// approxmiated total size of the local saved objects + target pages files (for user information)
        /// </summary>
        public long TotalSize { get; set; }

        public DateTime StartTime { get; set; }

        public void Reset()
        {
            StartTime = DateTime.Now;
            Objects = 0; Elements = 0; TotalSize = 0;

        }

        public void incObject(int newObjects, long newBytes = 0)
        {
            Objects += newObjects;
            TotalSize += newBytes;
        }
        public void incSize(long newBytes)
        {
            TotalSize += newBytes;
        }
        public void incElem(int number)
        {
            Elements += number;
        }
    }

}
