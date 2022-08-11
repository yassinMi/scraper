using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{
    public class MissingPluginException: Exception
    {
        public MissingPluginException(string s):base(s)
        {

        }
    }
}
