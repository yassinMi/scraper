using scraper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{

    public class Field
    {
        /// <summary>
        /// this must match the Model (type) property name as it's used in the DataGrid binding path
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// currently not used
        /// </summary>
        public Type NativeType { get; set; }
        /// <summary>
        /// will be used in maping properties to the right eement card positions 
        /// </summary>
        public FieldRole Role { get; set; }
        /// <summary>
        /// if not null this will override the tooltip text which is the UIName b default
        /// </summary>
        public string UserDescription { get; set; }
        /// <summary>
        /// not used yet
        /// future usage example: required fields having a null-like value will cause the row to be droped in cleaning processes
        /// </summary>
        public bool IsRequired { get; set; }
        private string _UIName;
        /// <summary>
        /// visible in the columnt header and other UI places, not setting this propety results in it being automaticall generated from the Name  
        /// </summary>
        public string UIName
        {
            get
            {
                if (_UIName == null)
                {
                    _UIName = CoreUtils.CamelCaseToUIText(Name);
                }
                return _UIName;
            }
            set
            {
                _UIName = value;
            }
        }
        /// <summary>
        /// the starting column width, (recommended width about 70), the user can always resize columns
        /// </summary>
        public int UIHeaderWidth { get; set; } = 70;
        /// <summary>
        /// fields disabled are not shown on the data grid while still saved and read from/into csv files (can also be used by the listView viewModel if role is matched
        /// </summary>
        public bool IsDataGridDisabled { get; set; }
    }



    public class ElementDescription
    {
        public IEnumerable<Field> Fields { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// two plugins with the same Element ID can co exist on the same workspace 
        /// the output data from the different plugis would be consistent and can be then loaded in the viewer without problems
        /// a good ID 
        /// </summary>
        public string ID { get; set; }
    }

}
