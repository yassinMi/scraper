﻿using Microsoft.VisualBasic.FileIO;
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
using System.Globalization;

namespace scraper.Services
{

    //@configurator way
    public class Synchronizer<T> 
    {
        private Dictionary<T, object> locks;
        private object myLock;

        public Synchronizer()
        {
            locks = new Dictionary<T, object>();
            myLock = new object();
        }

        public object this[T index]
        {
            get
            {
                lock (myLock)
                {
                    object result;
                    if (locks.TryGetValue(index, out result))
                        return result;

                    result = new object();
                    locks[index] = result;
                    return result;
                }
            }
        }
    }
    public static class Utils
    {
        //@deepee1
        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

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
                        isCurrentRowValid &= (fields.Count() == 11);
                        if (isCurrentRowValid == false) continue;
                        string name = fields[0];
                        string contactPerson = fields[1];
                        string address = fields[2];
                        string phonenumber = fields[3];
                        string email = fields[4];
                        string employees = fields[5];
                        string website = fields[6];
                        string year = fields[7];
                        string imageUrl = fields[8];
                        string link = fields[9];
                        string desc = fields[10];
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
        public static IEnumerable<Business> parseCSVfile_old(string path)
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
                    string contactPerson = fields[1];
                    string address = fields[2];
                    string phonenumber = fields[3];
                    string email = fields[4];
                    string employees = fields[5];
                    string website = fields[6];
                    string year = fields[7];
                    string imageUrl = fields[8];
                    string link = fields[9];
                    string desc = fields[10];
                    yield return new Business() {
                        company = name,
                        address = address,
                        contactPerson = contactPerson,
                        year = year,
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


        public static IEnumerable<Business> parseCSVfile(string path)
        {
            Debug.WriteLine("parsing csv: " + path);
            bool fileExixsts = File.Exists(path);
            Trace.Assert(fileExixsts, $"file '{path}' doesnt exist");
            if (fileExixsts == false)
            {
                return null;
            }
            List<Business> list = new List<Business>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                list =  csv.GetRecords<Business>().ToList();
            }

            return list;
            
                //company, contactPerson,address,phonenumber,email,employees,website,year,imageUrl,link,description
                /*return cr.EnumerateRecords(record).Select(r => new Business() {
                    company = r.company,
                    contactPerson = r.contactPerson,
                    address = r.address,
                    phonenumber = r.phonenumber,
                    email = r.email,
                    employees = r.employees,
                    year = r.year,
                    imageUrl = r.imageUrl,
                    link = r.link,
                    description = r.description
                }
                );*/

            
        }

    }
}
