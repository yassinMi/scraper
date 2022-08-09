using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{

    public enum FilterComponenetType { GroupFilter, RangeFilter}
    public class FilterComponenetDescription
    {
        public string Header { get; set; }
        public string PropertyName { get; set; }
        public FilterComponenetType Type {get;set;}

    }
}
