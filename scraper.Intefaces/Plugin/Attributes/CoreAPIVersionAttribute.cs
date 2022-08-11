using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Attributes
{
    /// <summary>
    /// Plugin assemblies must provide information on the API versison that they target so that future scraper versions with backwards incompatible core won't attempt to load them
    /// NOTE: the core assembly uses the same attribute class to declare it's version
    /// [i]
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class CoreAPIVersionAttribute : Attribute
    {
        public CoreAPIVersionAttribute(string version)
        {
            APIVersion = version ;
        }
        /// <summary>
        /// semVer with revision number ingnored, before major version 1 breaking changes are impllied with minor increase, after major 1 breaking changes are impllied with major number increase
        /// (these rules are implemented in the plugins manager)
        /// </summary>
        public string APIVersion { get; private set; }
    }
}
