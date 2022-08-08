using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    public abstract class FilterComponentBase
    {
        public string Header { get; protected set; }
        public abstract IEnumerable<T> Filter<T>(IEnumerable<T> input);
        /// <summary>
        /// used to update the internals that are dependant on the raw input collection such as the list of available groups
        /// </summary>
        /// <param name="input">the complete input collection (for better performance and UX the complete collection should be used that is the initial list on which the filters are applied)</param>
        public abstract void Update(IEnumerable<object> input);
    }

   
}
