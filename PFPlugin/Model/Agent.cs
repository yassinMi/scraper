using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFPlugin.Model
{
    /// <summary>
    /// has 13 props
    /// </summary>
    public class Agent
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Company { get; set; }
        public string TotalProperties { get; set; }
        public string YearsOfExperience { get; set; }
        public string Nationality { get; set; }
        public string Country { get; set; }
        public string isTrusted { get; set; }
        public string WhatsappResponseTime { get; set; }
        public string position { get; set; }
        public string BrokerName { get; set; }
        public string LinkedinAddress {get;set;}
        public string Image { get; set; }
    }
}
