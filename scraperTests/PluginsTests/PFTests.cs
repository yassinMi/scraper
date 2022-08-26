using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraperTests.PluginsTests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class PFTest
    {





        const string j = "{\"included\":[{\"type\":\"agent\",\"id\":\"161964\",\"attributes\":{\"user_id\":\"32485\",\"name\":\"DanielaGiannone\",\"image_token\":\"https://www.propertyfinder.ae/images/pf_agent/picture/d52219204050f9c2a4bf50f345faa950f548ea36/desktop\",\"phone\":\"+971569374836\",\"phone_user\":\"+971569374836\",\"phone_did\":null,\"phone_is_mobile\":true,\"position\":\"ClientManager\",\"biography\":\"LongtimeexperienceasMarketingandBusinessDevelopmentspecialistbothinItalyandUAE,primarilyintherealestatesector;\r\nStrongcustomercareattitudeintheleasingandpropertymanagementtoturntherelationshipbetweenlandlordsandtenantsintoafullysatisfactoryone.\r\nPeriodicalanalysisofRasAlKhaimahandUAErealestatesectortoanticipatefuturetrendsandtofindtherightsolutionforanyprospectiveinvestorand/orend-users.\r\nSocialmediaawarenessandpromotionincludingwritingandeditingofinvestmentopportunities,brochuresandcommercialflyers.\r\nDailyachievementspursuedthroughstrongabilityinnetworking,teamcooperation,tenacityandextremelypositiveattitudeinturning\"difficulties\"in\"opportunities\".\",\"smart_ad\":false,\"listing_level\":0,\"listing_level_label\":\"Standard\",\"nationality\":\"IT\",\"country_name\":\"Italy\",\"linkedin_address\":\"\",\"ranking\":68,\"are_transactions_visible\":true,\"agent_broker_license_no\":\"RAKIA74FZ301113582\",\"verification_status\":\"verified\",\"transactions_count\":0,\"total_properties\":17,\"properties_residential_for_rent_count\":9,\"properties_residential_for_sale_count\":8,\"properties_commercial_count\":0,\"years_of_experience\":12,\"is_trusted\":true,\"whatsapp_response_time_readable\":\"within5minutes\"},\"links\":{\"profile\":\"/en/agent/daniela-giannone-161964\",\"image_desktop\":\"https://www.propertyfinder.ae/images/pf_agent/picture/d52219204050f9c2a4bf50f345faa950f548ea36/desktop\"},\"meta\":{\"broker_id\":\"2964\",\"broker_name\":\"ProbimaCentreFZLLC-RAK\"}},{\"type\":\"location\",\"id\":\"3\",\"attributes\":{\"name\":\"RasAlKhaimah\",\"path\":\"3\",\"path_name\":\"\",\"location_type\":\"CITY\",\"review_score\":4.7142854,\"reviews_count\":3,\"image_token\":\"\",\"coordinates\":{\"lon\":55.970485,\"lat\":25.797942},\"level\":0,\"abbreviation\":\"\",\"current_language_slug\":\"ras-al-khaimah\",\"url_slug\":\"ras-al-khaimah\",\"children_count\":186}},{\"type\":\"location\",\"id\":\"151\",\"attributes\":{\"name\":\"AlHamraVillage\",\"path\":\"3.151\",\"path_name\":\"RasAlKhaimah\",\"location_type\":\"COMMUNITY\",\"review_score\":4.571429,\"reviews_count\":2,\"image_token\":\"\",\"coordinates\":{\"lon\":55.782853,\"lat\":25.691947},\"level\":1,\"abbreviation\":\"\",\"current_language_slug\":\"al-hamra-village\",\"url_slug\":\"al-hamra-village\",\"children_count\":36}}]}";



        string getJsonAgentsListingPayload(string doc)
        {
            string d1 = "window.propertyfinder.settings.property = {";
            string d2 = " form: ";
            var first_ix = doc.IndexOf(d1);
            var last_ix = doc.IndexOf(d2);
            string res = doc.Substring(first_ix + d1.Length, last_ix - (first_ix + d1.Length))
                .Trim().TrimEnd(',');
            string d1_ = "payload: ";
            var first_ix_ = res.IndexOf(d1_);
            res = res.Substring(first_ix_ + d1_.Length);
            Debug.WriteLine($"yass{res}yass");
            return res;
        }
        [TestMethod]
        public void exctractsAgentDetailsJson()
        {
            scraper.Core.Utils.CoreUtils.getUniqueLinkHash("https://www.propertyfinder.ae/en/find-agent/search");
            scraper.Core.Utils.CoreUtils.getUniqueLinkHash("https://www.propertyfinder.ae/en/find-agent/search?page=1");
            //string res = getJsonAgentsListingPayload(File.ReadAllText(@"F:\epicMyth-tmp-6-2022\freelancing\projects\pf\Daniela Giannone - Find 25 properties _ Property Finder UAE.html"));
            //Assert.IsTrue(res.StartsWith("{\"data\""));
            //Assert.IsTrue(res.EndsWith("\"per_page\":20}}"));
   
        }

        [TestMethod]
        public void parseAreas()
        {
            string res = getJsonAgentsListingPayload(File.ReadAllText(@"F:\epicMyth-tmp-6-2022\freelancing\projects\pf\Daniela Giannone - Find 25 properties _ Property Finder UAE.html"));

            //string a = PFPlugin.PFScrapingTask.parseAreas(res);
           // Assert.IsTrue(a== "Al Hamra Village • Al Mairid • Mina Al Arab", a);

        }
    }
}
