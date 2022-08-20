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




            Children.Add(new ExplorerFolder()
            {
                Name = "Temporary test categories",
                Children = new List<ExplorerElement>() {
                new ExplorerTarget(this) {
                    Name ="Reflective materials / goods",
                    URL ="https://2gis.ae/search/Reflective%20materials%20%2F%20goods/rubricId/57364",
                    ElementsCount=4
                },
                new ExplorerTarget(this) {
                    Name ="Special vehicle equipment",
                    URL ="https://2gis.ae/search/Special%20vehicle%20equipment/rubricId/433",
                    ElementsCount=70
                },
                new ExplorerTarget(this) {
                    Name ="Cooked food delivery",
                    URL ="https://2gis.ae/dubai/search/Cooked%20food%20delivery/rubricId/1203",
                    ElementsCount=533
                },

            }
            });

            
        }

        internal void OnStartRequested(Model.CategoryPicker.ExplorerTarget sourceTarget)
        {
            StartRequested?.Invoke(this, sourceTarget);
        }
        public event EventHandler<ExplorerTarget> StartRequested;




        public List<ExplorerElement> Children { get; set; } = new List<ExplorerElement>();
    }
}
