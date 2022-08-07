using BusinesslistPhPlugin;
using CsvHelper;
using HtmlAgilityPack;
using Mi.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using scraper.Core.Utils;
using scraper.Core.Workspace;
using scraper.Model;
using scraper.Plugin;
using scraper.Services;
using scraper.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scraper.ViewModel.Tests
{


    public class Fruit
    {

    }

    public class Apple : Fruit
    {

    }

    public class Orange : Fruit
    {

    }
    [TestClass()]
    public class MainViewModelTests
    {
       
       

       
        /// <summary>
        /// [ddfm]
        /// </summary>
        [TestMethod()]
        public void testInfosEnumerator()
        {
            /* Workspace.MakeCurrent(@"E:\TOOLS\scraper\tests.yass\blWorkspace");
             var raw = BLScrapingTask.downloadOrRead(@"https://www.businesslist.ph/company/301559/automation-and-security-inc",
             Workspace.Current.GetHtmlObjectsFolder());
             HtmlDocument doc = new HtmlDocument();
             doc.LoadHtml(raw);
             foreach (var item in BLScrapingTask.getInfos(doc.DocumentNode))
             {
                 Debug.WriteLine(item.Item1);
                 if (item.Item1 == "Employees" && item.Item4==true)
                 {
                     Debug.WriteLine(item.Item2);
                 }

             }

             return;*/

            foreach (var node in union())
            {
                foreach (var item in BLScrapingTask.getInfos(node.Value))
                {                 
                    if(item.Item1.ToLower()== "Location map".ToLower())
                    {
                        Debug.WriteLine(item.Item2);
                    }
                }
            }

            Debug.WriteLine($"union had {union().Count()} elements");

        }


        [TestMethod()]
        public void asm()
        {

            var recs = Enumerable.Range(0, 4).Select(s => new { e = 4, name = "yass" });
            
            Assert.IsTrue(recs.Count() == 4,"nope");

            using (var sw = new StreamWriter(@"E:\TOOLS\scraper\tests.yass\blWorkspace\tex.csv"))
            using (var csvw = new CsvWriter(sw, System.Globalization.CultureInfo.InvariantCulture))
            {
                csvw.WriteRecords(recs);
            }

           

            using (var sr = new StreamReader(@"E:\TOOLS\scraper\tests.yass\blWorkspace\tex.csv"))
                using(var csvr = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
            {
                var i = csvr.GetRecords(recs.First().GetType()).Count();
                Assert.IsTrue(i==4,$"got {i} not 4");
            }
                


        }













       






        [TestMethod()]
        public void testEnnumeratePages()
        {
           
        }



      


        public IEnumerable<KeyValuePair<string,HtmlNode>> union()
        {
            string t2 = @"E:\TOOLS\scraper\tests.yass\blWorkspace\tempTestsTargestPages";

            var t1 = @"E:\TOOLS\scraper\tests.yass\blWorkspace\companies-raw\html";

           return Directory.GetFiles(t1).Select(f =>
            {
                var res = File.ReadAllText(f);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);
                return  new KeyValuePair<string,HtmlNode>(f, doc.DocumentNode);
            });


        }


      
      



        [TestMethod()]
        public void parsng1()
        {
            string url1 = @"https://www.businesslist.ph/location/manila/3";
            string url2 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";
            string url3 = @"https://www.businesslist.ph/category/specialist-printing/city:manila";

            
        }




      
        

       



       
       
    }
}