using System;

namespace scraper.ViewModel.Tests
{
    public static class TestUrls
    {
        public static string GetRandomUrl()
        {
            return Urls  [new Random().Next(Urls.Length - 1)];
        }
        public static string[] Urls = {
"https://www.microcenter.com/search/search_results.aspx?N=4294967292&NTK=all&sortby=match&rpp=96&myStore=false",
"https://www.microcenter.com/search/search_results.aspx?N=4294967292&NTK=all&sortby=match&rpp=96&myStore=false&page=2",
"https://www.microcenter.com/search/search_results.aspx?N=4294967292&NTK=all&sortby=match&rpp=96&myStore=false&page=3",

"https://www.microcenter.com/search/search_results.aspx?N=4294966937&NTK=all&sortby=match&rpp=96&myStore=false",
"https://www.microcenter.com/search/search_results.aspx?N=4294966937&NTK=all&sortby=match&rpp=96&myStore=false&page=2",

"https://www.microcenter.com/search/search_results.aspx?N=4294966995+4294820410",
"https://www.microcenter.com/search/search_results.aspx?N=4294966995+4294814242",
"https://www.microcenter.com/search/search_results.aspx?N=4294966995+4294810695",


"https://www.microcenter.com/search/search_results.aspx?N=4294966800&NTK=all&sortby=match&rpp=96&myStore=false",
"https://www.microcenter.com/search/search_results.aspx?N=4294966800&NTK=all&sortby=match&rpp=96&myStore=false&page=2",
"https://www.microcenter.com/search/search_results.aspx?N=4294966800&NTK=all&sortby=match&rpp=96&myStore=false&page=3",
"https://www.microcenter.com/search/search_results.aspx?N=4294966800&NTK=all&sortby=match&rpp=96&myStore=false&page=4",


"https://www.microcenter.com/search/search_results.aspx?N=4294939904&NTK=all&sortby=match&rpp=96&myStore=false",
"https://www.microcenter.com/search/search_results.aspx?N=4294939904&NTK=all&sortby=match&rpp=96&myStore=false&page=2",
"https://www.microcenter.com/search/search_results.aspx?N=4294939904&NTK=all&sortby=match&rpp=96&myStore=false&page=3",
"https://www.microcenter.com/search/search_results.aspx?N=4294939904&NTK=all&sortby=match&rpp=96&myStore=false&page=4"
                };
    }
}