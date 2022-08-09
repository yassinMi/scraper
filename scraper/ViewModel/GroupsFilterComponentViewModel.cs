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
        public GroupsFilterComponentModel Model { get;  private set; }
        public GroupsFilterComponentViewModel(GroupsFilterComponentModel model)
        {
            Model = model;
            model.GroupsCollectionUpdated += (s, e) =>
            {
                notif(nameof(Groups));
            };
            
        }


        public IEnumerable<SelectableGroup> Groups
        {
            get { return Model.Groups; }
        }


    }
}
