using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model.CategoryPicker
{
    /// <summary>
    /// a 0 level category that can be scraped (contains elements)
    /// </summary>
    public class ExplorerTarget:ExplorerElement
    {
        public List<ExplorerElement> Children { get; set; }

        public string Name { get; set; }

        public ExplorerElementType Type { get { return ExplorerElementType.Target; } }

    }
}
