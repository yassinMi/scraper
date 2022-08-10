using scraper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    public class RangeFilterComponentModel : FilterComponentBase
    {
        FilterComponenetDescription originalDescriptor { get; set; }
        public RangeFilterComponentModel(FilterComponenetDescription desc)
        {
            originalDescriptor = desc;
        }
        public override IEnumerable<T> Filter<T>(IEnumerable<T> input)
        {
            throw new NotImplementedException();
        }

        private string min, max;
        public string Min
        {
            get
            {
                return min;
            }
            set
            {
                var val_res = originalDescriptor.MinMaxValidator(value);
                if (!val_res.Item1)
                {
                    ErrorMessage = val_res.Item2;
                    onErrorMessageChanged();
                    min = null;
                    return;
                }
                min = value;
                RangeParamsGotDirty?.Invoke(this, new EventArgs());
            }
        }

        public string ErrorMessage = null;
        public event EventHandler<string> Error;

        public event EventHandler RangeParamsGotDirty;

        void onErrorMessageChanged()
        {
            Error?.Invoke(this, ErrorMessage);
        }

        public string Max
        {
            get
            {
                return max;
            }
            set
            {
                var val_res = originalDescriptor.MinMaxValidator(value);
                if (!val_res.Item1)
                {
                    ErrorMessage = val_res.Item2;
                    onErrorMessageChanged();
                    max = null;
                    return;
                }
                
                max = value;
                RangeParamsGotDirty?.Invoke(this, new EventArgs());
            }
        }


        public override bool Passes(object element)
        {
            if (Min == null || Max == null) return true;
            try
            {
                return originalDescriptor.IsInRange(Min, Max, element);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"IsInRnage delegete threw an exception: {e.GetType()}:{e.Message}");
                return true;
            }
            
        }

        public override void Update(IEnumerable<object> input)
        {
            //pass
        }
    }
}
