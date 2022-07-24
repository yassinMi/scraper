using CsvHelper;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper.Core.Utils
{
    public static class CSVUtils
    {
        /// <summary>
        /// used to both count and check data validity
        /// </summary>
        /// <param name="path">must exist</param>
        /// <returns></returns>
        public static bool checkCSV(string path, out int rowsCount, out int validRowsCount)
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
                return validRowsCount == 0;
            }
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
        public static void CSVOverwriteRecords(string filename, IEnumerable records)
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




        public static IEnumerable<T> parseCSVfile<T>(string path)
        {
            Debug.WriteLine("parsing csv: " + path);
            bool fileExixsts = File.Exists(path);
            Trace.Assert(fileExixsts, $"file '{path}' doesnt exist");
            if (fileExixsts == false)
            {
                return null;
            }
            List<T> list = new List<T>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                list = csv.GetRecords<T>().ToList();
            }

            return list;
        }
        public static IEnumerable parseCSVfile(Type recordType, string path)
        {
            Debug.WriteLine("parsing csv: " + path);
            bool fileExixsts = File.Exists(path);
            Trace.Assert(fileExixsts, $"file '{path}' doesnt exist");
            if (fileExixsts == false)
            {
                return null;
            }
            List<object> list = new List<object>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                list = csv.GetRecords(recordType).ToList();
            }

            return list;
        }

    }
}
