using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    public enum FilteringRuleType { IsNotEmpty, Equals, Contains }
    public class FilteringRule
    {
        public string fieldName { get; set; }
        public FilteringRuleType RuleTtype { get; set; }
        public string RuleParam { get; set; }
        /// <summary>
        /// check the rule against an element, returns true indcats the element should be selected
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Check(Business b)
        {
            object value = b.GetType().GetProperty(fieldName).GetValue(b);
            switch (RuleTtype)
            {
                case FilteringRuleType.IsNotEmpty:
                    if (value is string)
                    {
                        return string.IsNullOrWhiteSpace((string)value) == false && ((string)value).ToLower() != "n/a";
                    }
                    else if (value is int) return (value != null && ((int)value) != 0);
                    else return true;
                case FilteringRuleType.Equals:
                    return value == ConvertParamStringToTargetPropertyType(b.GetType());
                case FilteringRuleType.Contains:
                    return (value is string) && ((string)value).Contains(RuleParam);
                default:
                    return true;
            }
        }

        public override string ToString()
        {
            switch (RuleTtype)
            {
                case FilteringRuleType.IsNotEmpty:
                    return $"{fieldName} is not empty";
                case FilteringRuleType.Equals:
                    return $"{fieldName} = {RuleParam}";
                    
                case FilteringRuleType.Contains:
                    return $"{fieldName} contains {RuleParam}";
                default:
                    return "";
                    
            }
        }

        private object ConvertParamStringToTargetPropertyType(Type targetObjectType)
        {
            var prop = targetObjectType.GetProperty(fieldName);
            if (prop == null) return null;
            if(prop.PropertyType == typeof(string))
            {
                return RuleParam;
            }
            else if (prop.PropertyType == typeof(int))
            {
                int res = 0;
                if (int.TryParse(RuleParam, out res) == false) return null;
                return res;
            }
            else
            {
                return null;
            }
            }

        

        
    }
}
