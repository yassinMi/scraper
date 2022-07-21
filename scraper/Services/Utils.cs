using Microsoft.VisualBasic.FileIO;
using scraper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Collections;
using System.IO;

namespace scraper.Services
{
    public static class Utils
    {


        public static void CSVAppendRecords(string filename, IEnumerable records)
        {
            var cof = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            using (var writer = new StreamWriter(filename, true))
            using (var csv = new CsvWriter(writer, cof))
            {
                csv.WriteRecords(records);
            }
        }
        public static void CSVOverwriteRecords(string filename,IEnumerable records )
        {
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }

        public static void CSVWriteRecords(string filename, IEnumerable records, bool appendOrOverwrite)
        {
            if (appendOrOverwrite) CSVAppendRecords(filename, records);
            else CSVOverwriteRecords(filename, records);
        }


        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);


                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }


        public static string StripHTML(string htmlStr)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlStr);
            var root = doc.DocumentNode;
            string s = "";
            foreach (var node in root.DescendantsAndSelf())
            {
                if (!node.HasChildNodes)
                {
                    string text = node.InnerText;
                    if (!string.IsNullOrEmpty(text))
                        s += text.Trim() + " ";
                }
            }
            return s.Trim();
        }


        private static readonly Regex InvalidFileRegex = new Regex(
    string.Format("[{0}]", Regex.Escape(@"<>:""/\|?*")));

        public static string SanitizeFileName(string fileName)
        {
            return InvalidFileRegex.Replace(fileName, string.Empty);
        }

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
            Debug.WriteLine("parsing csv: " + path);
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
                        //as : name,address,phonenumber,email,employees,website,imageUrl,link,description, id?
                        string[] fields = csvParser.ReadFields();
                        isCurrentRowValid &= (fields.Count() == 10);
                        if (isCurrentRowValid == false) continue;
                        string name = fields[0];
                        string address = fields[1];
                        string phonenumber = fields[2];
                        string email = fields[3];
                        string employees = fields[4];
                        string website = fields[5];
                        string imageUrl = fields[6];
                        string link = fields[7];
                        string desc = fields[8];
                        isCurrentRowValid &= name.Length > 0 && address.Length > 0 && name.Length > 0 && link.Length > 0 && link.Length > 0 && link.Length > 0;

                       
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
        public static IEnumerable<Business> parseCSVfile(string path)
        {
            Debug.WriteLine("parsing csv: " + path);
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
                    /*name;
        public string description;
        public string website;
        public string phonenumber, email;
        public string link;
        public string employees;
        public string imageUrl;*/
                    //as : name,address,phonenumber,email,employees,website,imageUrl,id
                    string[] fields = csvParser.ReadFields();
                    string name = fields[0];
                    string address = fields[1];
                    string phonenumber = fields[2];
                    string email = fields[3];
                    string employees = fields[4];
                    string website = fields[5];
                    string imageUrl = fields[6];
                    string link = fields[7];
                    string desc = fields[8];
                    string id = fields[9];
                    yield return new Business() {
                        name = name,
                        address = address,
                        phonenumber = phonenumber,
                        email = email,
                        employees = employees,
                        website = website,
                        imageUrl = imageUrl,
                        link = link,
                        
                    };
                }
            }
        }

    }
}
