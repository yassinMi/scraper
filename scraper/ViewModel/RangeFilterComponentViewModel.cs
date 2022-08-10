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
            

        }


        private string _ValidationErrorMessage=null;
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
            notif(nameof(Header));
            ((RangeFilterComponentModel)Model).Error += (s, e) =>
            {
                ValidationErrorMessage = e;
                Debug.WriteLine(e);
                notif(ValidationErrorMessage);
                Debug.WriteLine(ValidationErrorMessage);
            };
        }

        public override string Header
        {
            get { return Model.Header; }
        }

    }
}
