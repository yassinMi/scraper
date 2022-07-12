using Microsoft.VisualBasic.FileIO;
using scraper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace scraper.Services
{
    public static class Utils
    {

        public static Process constructProcess(string filename, string args, string workingDir)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = filename;
            startInfo.Arguments = args;
            startInfo.WorkingDirectory = workingDir;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            return process;
        }

        /// <summary>
        /// used to both count and check data validity
        /// </summary>
        /// <param name="path">must exist</param>
        /// <returns></returns>
        public static bool checkCSV(string path,  out int rowsCount, out int validRowsCount)
        {
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                rowsCount = 0;
                validRowsCount = 0;
                csvParser.CommentTokens = new string[] { "#" }; csvParser.SetDelimiters(new string[] { "," }); csvParser.HasFieldsEnclosedInQuotes = true;
                // first row only contains column names  id,title,upc,sku,price,img,link
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    bool isCurrentRowValid = true;
                    rowsCount++;
                    try
                    {
                        string[] fields = csvParser.ReadFields();
                        isCurrentRowValid &= (fields.Count() == 7);
                        string id = fields[0];
                        string title = fields[1];
                        string upc = fields[2];
                        string sku = fields[3];
                        string price = fields[4];
                        string img = fields[5];
                        string link = fields[6];
                        double price_d = 0;
                        isCurrentRowValid &= id.Length > 0 && title.Length > 0 && upc.Length > 0 && sku.Length > 0 && img.Length > 0 && link.Length > 0;

                        isCurrentRowValid &= double.TryParse(price, out price_d);
                        if (isCurrentRowValid)
                        {
                            validRowsCount++;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return  validRowsCount==0;
            }
        }




        /// <summary>
        /// returns a IEnumerable, doesn't load the whole thing into memory
        /// expection unsafe
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<Product> parseCSVfile(string path)
        {
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                // first row only contains column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    //parsing current row
                    //as : id,title,upc,sku,price,img,link
                    string[] fields = csvParser.ReadFields();
                    string id = fields[0];
                    string title = fields[1];
                    string upc = fields[2];
                    string sku = fields[3];
                    string price = fields[4];
                    string img = fields[5];
                    string link = fields[6];
                    double price_d = 0;
                    double.TryParse(price, out price_d);
                    yield return new Product() { ID = (ProductID) id, title = title, upc = upc, sku = sku, price = price_d, imageUrl = img, link = link };
                }
            }
        }

    }
}
