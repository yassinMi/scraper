using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model.CategoryPicker
{
    /// <summary>
    /// a container for other ExplorerElements 
    /// </summary>
    public class ExplorerFolder : ExplorerElement
    {
        public List<ExplorerElement> Children { get; set; }

        public string Name { get; set; }

        public ExplorerElementType Type { get { return ExplorerElementType.Folder; } }
    }
}
