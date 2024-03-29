﻿using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{
    public static class BLScraperTestsData
    {

        public static string pluginName = "Businesslist.ph scraper";
        public static string bl_tests_ws_path =  @"E:\TOOLS\scraper\tests.yass\blWorkspace";
       
        public static List<TargetPageBLScraperTestData> KnownPages()
        {
            List<TargetPageBLScraperTestData> res = new List<TargetPageBLScraperTestData>();


            res.Add(new TargetPageBLScraperTestData() {
                targetPageUrl = @"https://www.businesslist.ph/category/industrial-premises",
                elementsCount = -1,
                csvRef = @"E:\TOOLS\scraper\tests.yass\blWorkspace\cs.ref\Top Industrial Premises in Philippines.csv",
                pagesCount = 4
            });



            res.Add(new TargetPageBLScraperTestData()
            {
                targetPageUrl = @"https://www.businesslist.ph/location/santa-rosa-city",
                elementsCount = -1,
                csvRef = @"E:\TOOLS\scraper\tests.yass\blWorkspace\cs.ref\Companies in Santa Rosa City, Philippines.csv",
                pagesCount = 9
            });


            return res;

        }
    }
}
