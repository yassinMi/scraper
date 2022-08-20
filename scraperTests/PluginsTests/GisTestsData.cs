using scraper.Core.Utils;
using scraper.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{
    public static class GisestsData
    {

        public static string bl_tests_ws_path =  @"E:\TOOLS\scraper\tests.yass\GisWsAuto";
       
        



        public static List<KeyValuePair<string,bool>> knownTPValidatorData()
        {
            var res= new List<KeyValuePair<string, bool>>();

            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/search/Special%20vehicle%20equipment/rubricId/433",
                true
                ));

            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/search/Fit-out%20works/rubricId/276",
                true
                ));

            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/dubai/search/Housing%20and%20communal%20services/rubricId/758/page/2/firm/70000001019185736/55.196621%2C25.117124",
                true
                ));

            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/dubai/rubrics/subrubrics/110553",
                false
                ));

            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/search/technical%20services%20company/page/2",
                true
                ));
            res.Add(new KeyValuePair<string, bool>(
                "https://2gis.ae/dubai/search/Mobile%20network%20operators/rubricId/337",
                true
                ));

            res.Add(new KeyValuePair<string, bool>(
              "https://2gzis.ae/dubai/search/Mobile%20network%20operators/rubricId/337",
              false
              ));

            res.Add(new KeyValuePair<string, bool>(
              "https://2gis.ae/search/Reflective%20materials%20%2F%20goods/rubricId/57364",
              true
              ));

            //



            return res;
        }
    }
}
