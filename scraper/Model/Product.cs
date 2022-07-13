using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Model
{
    public struct Rating
    {
        int stars_cc, total_stars, vote_count;
    }
    public struct ProductID: IElementID
    {
        public string id { get; set; }
        public static implicit operator ProductID (string x) => new ProductID(x);
        public ProductID(string id) 
        {
            this.id = id;            
        }
        public override string ToString()
        {
            return id;
        }
    }


    public class Product : IElement
    {

        public IElementID ID { get; set; }

        public double price;
        public string title;
        public string website;
        public string upc, sku;
        public string link;
        public Rating rating;
        public bool isAvailableOnAmazon;
        public double amazonPrice;
        public string amazonLink;
        public string imageUrl;

        
    }
}
