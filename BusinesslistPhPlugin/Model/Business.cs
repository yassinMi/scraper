﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinesslistPhPlugin.Model
{
    public struct Rating
    {
        int stars_cc, total_stars, vote_count;
    }
   
    public class Business 
    {
        //as : name,address,phonenumber,email,employees,website,imageUrl,link,description, id?


        public string company { get; set; }
        public string contactPerson { get; set; }
        public string address { get; set; }
        public string phonenumber { get; set; }
        public string email { get; set; }
        public string employees { get; set; }
        public string website { get; set; }
        public string year { get; set; }
        public string imageUrl { get; set; }
        public string link { get; set; }
        public string description { get; set; }



    }
}
