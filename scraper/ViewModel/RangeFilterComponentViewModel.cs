using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;

namespace scraper.ViewModel
{
    public class RangeFilterComponentViewModel : BaseFilterComponentViewModel
    {
        public RangeFilterComponentViewModel()
        {
            //design time only ctor   
            notif(nameof(Header));
            (Model as RangeFilterComponentModel).Error += (s, e) =>
            {
                ValidationErrorMessage = e;
            };

        }


        private string _ValidationErrorMessage;
        public string ValidationErrorMessage
        {
            set { _ValidationErrorMessage = value; notif(nameof(ValidationErrorMessage)); }
            get { return _ValidationErrorMessage; }
        }


        private string _Min;
        public string Min
        {
            set {
                _Min = value; notif(nameof(Min));
                ValidationErrorMessage = null;
                (Model as RangeFilterComponentModel).Min = value;

            }
            get { return _Min; }
        }


        private string _Max;
        public string Max
        {
            set { _Max = value;
                ValidationErrorMessage = null;
                (Model as RangeFilterComponentModel).Max = value;
                notif(nameof(Max));
            }
            get { return _Max; }
        }


        public RangeFilterComponentViewModel(RangeFilterComponentModel model)
        {
            this.Model = model;
        }

        public override string Header
        {
            get { return Model.Header; }
        }

    }
}
