using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model.CategoryPicker
{

    public enum ExplorerElementType { Folder, Target }
    public interface  ExplorerElement
    {
        string Name { get; set; }
        ExplorerElementType Type { get; }
        List<ExplorerElement> Children { get; }
    }
}
