using scraper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace scraper.Core
{

    
    
   

    /// <summary>
    /// supported pattern is: download a page's static elements
    /// </summary>
    public abstract class Plugin
    {
        public abstract ScrapingTaskBase GetTask(string targetPage);
        public abstract ScrapingTaskBase GetTask(TaskInfo taskInfo);
        public abstract string Name { get; }
        public virtual Version Version { get; } = new Version(0, 0);
        public virtual string ElementName { get; } = "Element";
        public virtual string ElementNamePlural { get; } = "Elements";
        private ElementDescription _ElementDescription;
        public virtual ElementDescription ElementDescription
        {
            get
            {
                if (_ElementDescription == null)
                {

                    _ElementDescription = new ElementDescription()
                    {
                        Fields = ElementModelType.GetProperties().Select(p => new Field()
                        {
                            Name = p.Name,
                            NativeType = p.PropertyType,
                            UIName = CoreUtils.CamelCaseToUIText(p.Name)

                        }),
                        ID = ElementName,
                        Name = ElementName
                    };

                }
                return _ElementDescription;
            }
            set
            {
                _ElementDescription = value;
            }
        }
        public abstract Type ElementModelType { get; }
        public virtual PluginUsageInfo UsageInfo { get; } = null;
        /// <summary>
        /// example:  
        /// if none null it will be part of the default targetPage input Validation function
        /// </summary>
        public abstract string TargetHost { get; }

        public virtual bool ValidateTargetPageInputQuery(string input)
        {
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri) == false)
            {
                return false;
            }
            string website = uri.Host.ToLower();
            if ((TargetHost != null) && !((website == TargetHost))) return false;
            return true;
        }
    }






}
