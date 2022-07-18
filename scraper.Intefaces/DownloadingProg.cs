using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{
    public struct DownloadingProg
    {
        public int Total, Current;
        public double PercentageNormalized { get { return (double)Current / Total; } }
        public double Percentage { get { return 100d * (double)Current / Total; } }
        public override string ToString()
        {
            return string.Format("{0}/{1}", Current, Total);
        }

    }
    
}
