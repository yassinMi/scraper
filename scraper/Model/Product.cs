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


    public class Business : IElement
    {
        //as : name,address,phonenumber,email,employees,website,imageUrl,link,description, id?


        public string name { get; set; }
        public string address { get; set; }
        public string phonenumber { get; set; }
        public string email { get; set; }
        public string employees { get; set; }
        public string website { get; set; }
        public string imageUrl { get; set; }
        public string link { get; set; }
        public string description { get; set; }
        public IElementID ID { get; set; }



    }
}
