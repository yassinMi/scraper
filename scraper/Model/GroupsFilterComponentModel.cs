﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace scraper.Model
{

    public class SelectableGroup 
    {
        private GroupsFilterComponentModel ContainerComponent;
        public SelectableGroup(GroupsFilterComponentModel containerComponent)
        {
            ContainerComponent = containerComponent;
        }
        
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
                ContainerComponent.onDirtyGroupsSelection();
                
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
        
    
        public GroupsFilterComponentModel(Type modelType, string groupByPropertyName,string header=null)
        {
            ModelType = modelType; GroupByPropertyName = groupByPropertyName;
            Header = header??Core.Utils.CoreUtils.CamelCaseToUIText(groupByPropertyName + " filter");
        }

        public GroupsFilterComponentModel(List<SelectableGroup> groups)
        {
            
            Groups = groups;
            Header = "design time filter";
            
        }

        /// <summary>
        /// this is populated with the values from the property specified by the groupByPropertyName ctor param. the ToString method is used to convert these values in case thei're not strings.
        /// </summary>
        public List<SelectableGroup> Groups { get; private set; }
        public event EventHandler<IEnumerable<SelectableGroup>> GroupsCollectionUpdated;
        public event EventHandler<IEnumerable<SelectableGroup>> GroupsSelectionGotDirty;
        public override void Update(IEnumerable<object> input)
        {
            if (input == null || !input.Any()) {
                Groups = new List<SelectableGroup>();
                GroupsCollectionUpdated?.Invoke(this, Groups);
                return;
            };
            if (input.FirstOrDefault().GetType() != ModelType)
            {
                Debug.WriteLine($"Objects of type {input.FirstOrDefault().GetType()} does not match expected model type {ModelType }");
                return ;
            }

            var grps = input.GroupBy(o => ModelType.GetProperty(GroupByPropertyName).GetValue(o));
            SelectableGroup[] previouslyDisabledGroups = null;
            if (Groups!=null && Groups.Any())
            {
                //preserving the groups disabled state for better UX 
                previouslyDisabledGroups = Groups.Where(g => g.IsSelected == false).ToArray();
            }
            Groups = grps.Select(g => new SelectableGroup(this) { Name = g.Key.ToString(), IsSelected = (previouslyDisabledGroups!=null)&& previouslyDisabledGroups.Any(d=>d.Name==g.Key.ToString())? false :true }).ToList();
           
            GroupsCollectionUpdated?.Invoke(this, Groups);
        }

        /// <summary>
        /// compaires the group value with the elemet's property value using toString 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override bool Passes(object element)
        {
            if (Groups == null || !Groups.Any()) return true;
            if (element == null)
            {
                Debug.WriteLine($"Object cannot be null");
                return false;
            }
            if (element.GetType() != ModelType)
            {
                Debug.WriteLine($"Object of type {element.GetType()} does not match expected model type {ModelType }");
                return false;
            }
            var activeGroups = Groups.Where(g => g.IsSelected);
            //Debug.WriteLine($"{activeGroups.Count()} groups are selected coparint against: {ModelType.GetProperty(GroupByPropertyName).GetValue(element).ToString()}");
             return activeGroups.Any(g =>  (g.Name == ModelType.GetProperty(GroupByPropertyName).GetValue(element).ToString()));

            
        }

        /// <summary>
        /// not used ,use passes() as a predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public override IEnumerable<T> Filter<T>(IEnumerable<T> input)
        {
            return null;
        }

        internal void onDirtyGroupsSelection()
        {
            GroupsSelectionGotDirty?.Invoke(this, Groups);
        }
    }

}
