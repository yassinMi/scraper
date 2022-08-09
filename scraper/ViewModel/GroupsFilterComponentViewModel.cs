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
    public class GroupsFilterComponentViewModel : BaseFilterComponentViewModel
    {
        public GroupsFilterComponentViewModel()
        {

            //design time only ctor
           
            

            var Groups = new List<SelectableGroup>()
           {
                
           };
            GroupsFilterComponentModel m = new GroupsFilterComponentModel(Groups);
            Groups.AddRange(new SelectableGroup[]
            {
                new SelectableGroup(m) { IsSelected= true, Name = "Design time group 1" },
                new SelectableGroup(m) { IsSelected= false, Name = "Dt group 2" },
                new SelectableGroup(m) { IsSelected= true, Name = "Dt group 3" },
                new SelectableGroup(m) { IsSelected= false, Name = "Dt group 4" },
                new SelectableGroup(m) { IsSelected= true, Name = "Dt group 5 with long title to test the layout" },
            });
            notif(nameof(Groups));
            notif(nameof(Header));

        }
        public GroupsFilterComponentViewModel(GroupsFilterComponentModel model)
        {
            this.Model = model;
            model.GroupsCollectionUpdated += (s, e) =>
            {
                notif(nameof(Groups));
            };
            
        }

        public override string Header
        {
            get { return Model.Header; }
        }
            



        public IEnumerable<SelectableGroup> Groups
        {
            get { return ((GroupsFilterComponentModel) Model).Groups; }
        }


    }
}
