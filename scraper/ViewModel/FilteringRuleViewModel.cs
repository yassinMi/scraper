using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;
using System.Windows.Input;
using Mi.Common;
using System.Diagnostics;

namespace scraper.ViewModel
{
    public class FilteringRuleViewModel:BaseViewModel
    {
        public FilteringRuleViewModel(Model.FilteringRule m)
        {
            this.Model = m;
            notif(nameof(Caption));
        }

        public event EventHandler OnDeleteRequest;

         public FilteringRule Model { get; private set; }

        

        public bool IsRuleTypeRequiresParam { get
            {
                return Model.RuleTtype == FilteringRuleType.Contains || Model.RuleTtype == FilteringRuleType.Equals;
            } }

        public string Caption { get
            {
                return Model.ToString();
            } }

        public ICommand DeleteCommand { get { return new MICommand(hndlDeleteCommand); } }

        private void hndlDeleteCommand()
        {
            Debug.WriteLine("deleting");
            OnDeleteRequest?.Invoke(this, new EventArgs());
        }
    }
}
