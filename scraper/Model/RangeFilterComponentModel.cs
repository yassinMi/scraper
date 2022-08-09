using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    public class RangeFilterComponentModel : FilterComponentBase
    {
        public override IEnumerable<T> Filter<T>(IEnumerable<T> input)
        {
            throw new NotImplementedException();
        }

        public override bool Passes(object element)
        {
            throw new NotImplementedException();
        }

        public override void Update(IEnumerable<object> input)
        {
            throw new NotImplementedException();
        }
    }
}
