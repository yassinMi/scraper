using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{

    public interface IElementID
    {
        /// <summary>
        /// unique among all elements, should meet filenaame constrains
        /// </summary>
        string id { get; set; }
        
    }
    /// <summary>
    /// the basic data structure that represent one element eg could be a product, video, book, the plugn will carry the details
    /// </summary>
    public interface IElement
    {
        IElementID ID { get; set; }



    }
}
