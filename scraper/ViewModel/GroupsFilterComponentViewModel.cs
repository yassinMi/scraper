using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.Model;

namespace scraper.ViewModel
{
    public class GroupsFilterComponentViewModel : BaseViewModel
    {
        public GroupsFilterComponentViewModel(GroupsFilterComponentModel model)
        {
            Groups = new string[] { "group 1", "group 2", "group 3", "group 4 long name to test ui", }.Select(s=>new SelectableGroup() {Name = s, IsSelected = true });
        }


        private IEnumerable<SelectableGroup> _Groups;
        public IEnumerable<SelectableGroup> Groups
        {
            set { _Groups = value; notif(nameof(Groups)); }
            get { return _Groups; }
        }


    }
}
