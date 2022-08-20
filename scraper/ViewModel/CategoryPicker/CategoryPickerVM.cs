using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scraper.ViewModel;
using scraper.Model.CategoryPicker;

namespace scraper.ViewModel.CategoryPicker
{
    public class CategoryPickerVM:BaseViewModel
    {
        public CategoryPickerVM()
        {
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 3", Children= new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            } });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder()
            {
                Name = "my folder 3",
                Children = new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            }
            });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder()
            {
                Name = "my folder 3",
                Children = new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            }
            });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder()
            {
                Name = "my folder 3",
                Children = new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            }
            });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder()
            {
                Name = "my folder 3",
                Children = new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            }
            });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder() { Name = "my folder 1" });
            Children.Add(new ExplorerFolder()
            {
                Name = "my folder 3",
                Children = new List<ExplorerElement>() {
                new ExplorerFolder() {Name="sub 1" },
                 new ExplorerTarget() {Name="tar 1" },
            }
            });
        }

        public List<ExplorerElement> Children { get; set; } = new List<ExplorerElement>();
    }
}
