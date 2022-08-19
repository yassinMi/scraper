using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{
    [TestClass]
    public class GisTests
    {
        const string LIST1 = @"file:///F:/epicMyth-tmp-6-2022/freelancing/projects/gis/technical services company — 2GIS.html";

        const string ELEMENT_CHACHED = @"file:///F:/epicMyth-tmp-6-2022/freelancing/projects/gis/Al Falaq Technical Services Company, Al Rashdaan Building, 67, Al Muteena Street, Dubai — 2GIS.html";
        [TestMethod]
        public void getPhoneTest()
        {
            using (ChromeDriver mwd = new ChromeDriver())
            {
                mwd.Url = ELEMENT_CHACHED;
                string phone = null;
                mwd.Navigate();
                try
                {
                    phone = TwoGisPlugin.TwoGisScrapingTask.getPhone(mwd.FindElement(By.ClassName("_1rkbbi0x")));
                    phone = phone.Replace("tel:", "");

                }
                catch (Exception err)
                {
                    Debug.WriteLine(err);
                }
                string correct_phone = "+97142398771";
                Assert.IsTrue((phone) == correct_phone, $"excpected {correct_phone} got {phone}");

            }

        }


        [TestMethod]
        public void getLocTest()
        {
            using (ChromeDriver mwd = new ChromeDriver())
            {
                mwd.Url = LIST1;
                string phone = null;
                mwd.Navigate();
                
               
               
            }

        }
    }
}
