using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{

    public class SelectableGroup 
    {
        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                Debug.WriteLine($"{Name} got {(value ? "selected" : "unselected")}");
            }
        }
        public string Name { get; set; }
        public override string ToString()
        {
            return $"Grp:{Name}";
        }
    }

    public sealed class GroupsFilterComponentModel : FilterComponentBase
    {
        Type ModelType;
        string GroupByPropertyName;
        public GroupsFilterComponentModel(Type modelType, string groupByPropertyName)
        {
            ModelType = modelType; GroupByPropertyName = groupByPropertyName;
            Header = Core.Utils.CoreUtils.CamelCaseToUIText(groupByPropertyName + " filter");
        }
        /// <summary>
        /// this is populated with the values from the property specified by the groupByPropertyName ctor param. the ToString method is used to convert these values in case thei're not strings.
        /// </summary>
        public IEnumerable<SelectableGroup> Groups { get; private set; }
        public event EventHandler<IEnumerable<string>> GroupsCollectionUpdated;
        public override void Update(IEnumerable<object> input)
        {

            var grps = input.GroupBy(o => ModelType.GetProperty(GroupByPropertyName).GetValue(o));
            Groups = grps.Select(g => new SelectableGroup() { Name = g.Key.ToString(), IsSelected = false });

        }

        public override IEnumerable<T> Filter<T>(IEnumerable<T> input)
        {
            return null;
        }
    }

}
