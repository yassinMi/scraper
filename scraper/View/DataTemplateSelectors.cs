using scraper.Model.CategoryPicker;
using scraper.ViewModel;
using scraper.ViewModel.CategoryPicker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace scraper.View.TemplateSelectors
{


    


    public class ExplorerTreeItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elem = container as FrameworkElement;
            DataTemplate FolderTemplate = ((DataTemplate)elem.FindResource("TreeHeaderTemplate_Folder"));
            DataTemplate FileTemplate = ((DataTemplate)elem.FindResource("TreeHeaderTemplate_Target"));
            if (item != null && item is ExplorerElement)//nt type when implemented
            {
                switch (((ExplorerElement)item).Type)
                {
                    case  ExplorerElementType.Folder:
                        return FolderTemplate;
                    case  ExplorerElementType.Target:
                        return FileTemplate;
                    default:
                        return null;
                }
            }
            else return null;

        }
    }

}
