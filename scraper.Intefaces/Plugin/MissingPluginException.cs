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

    public class WorkspaceNotFoundException : Exception
    {
        public WorkspaceNotFoundException(string s):base(s)
        {

        }
    }
}
