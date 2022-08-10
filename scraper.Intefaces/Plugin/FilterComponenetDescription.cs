using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core
{

    public enum FilterComponenetType { GroupFilter, RangeFilter}
    public class FilterComponenetDescription
    {
        public string Header { get; set; }
        /// <summary>
        /// apllies for both Range and Groups filter type, determines the model targted property by name
        /// </summary>
        public string PropertyName { get; set; }
        public FilterComponenetType Type { get; set; }

        /// <summary>
        /// called when the user changes the Min or Max parameter, must return must return a value indicating whether the user input is valid
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorMessage">the UI error message to display</param>
        /// <returns></returns>
        public delegate bool ValidateMinMaxParameterHandler(string input, out string errorMessage);

        /// <summary>
        /// predicate used to filter the elements
        /// </summary>
        /// <param name="obj">model instance</param>
        /// <returns></returns>
        public delegate bool IsInRangeHnadler(string min, string max, object obj);

        /// <summary>
        /// only used for range filter type
        /// </summary>
        public ValidateMinMaxParameterHandler MinMaxValidator {get;set;}
        /// <summary>
        /// only used for range filter type
        /// </summary>
        public IsInRangeHnadler IsInRange { get; set; }



    }
}
