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
        /// [non generic]
        /// used to both count and check data validity
        /// </summary>
        /// <param name="path">must exist</param>
        /// <returns></returns>
        public static bool checkCSV(string path, Plugin plugin, out int rowsCount, out int validRowsCount)
        {
            Debug.WriteLine("parsing csv: " + path);
            //# validating header
            using (var reader = new StreamReader(path))
                try
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        csv.ValidateHeader(plugin.ElementModelType);
                    }
                }
                catch (Exception err) when (err is CsvHelper.ReaderException ||
                err is HeaderValidationException
                )
                {
                    rowsCount = 0; validRowsCount = 0;
                    return false;
                }
        

            //# validatinng rows: format is considered bad if no valid rows found
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
                        int ed_fields_cc = plugin.ElementDescription.Fields.Count();
                        isCurrentRowValid &= (fields.Count() == ed_fields_cc);
                        if (isCurrentRowValid == false) {
                            Debug.WriteLine($"bad csv format: {fields.Count()} fields exist while {ed_fields_cc} are expected in csv resource '{path}'");
                            continue;
                        };

                        byte f_ix = 0;
                        //company,contactPerson,address,phonenumber,email,employees,website,year,imageUrl,link,description
                        foreach (var f in plugin.ElementDescription.Fields)
                        {
                            if (f.IsRequired)
                            {
                                if(string.IsNullOrWhiteSpace(fields[f_ix]))
                                {
                                    isCurrentRowValid = false;
                                    Debug.WriteLine($"required field '{f.Name}' has empty valuein csv resource '{path}'");
                                    continue;
                                }
                            }
                            f_ix++;
                        }
                        
                        /*string address = fields[2];
                        string phonenumber = fields[3];
                        string email = fields[4];
                        string employees = fields[5];
                        string website = fields[6];
                        string year = fields[7];
                        string imageUrl = fields[8];
                        string link = fields[9];
                        string desc = fields[10];*/


                        if (isCurrentRowValid)
                        {
                            validRowsCount++;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return validRowsCount != 0;
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
        /// <summary>
        /// returns null when bad data or missing file
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<object> parseCSVfile(Type recordType, string path)
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
                try
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        list = csv.GetRecords(recordType).ToList();
                    }
                }
                catch (Exception err) when (err is HeaderValidationException 
                || err is CsvHelper.MissingFieldException)
                {
                    return null;
                }
            

            return list;
        }

    }
}
