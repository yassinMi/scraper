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
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows;

namespace scraper.Services
{


    public static class Utils
    {

        public static MessageBoxImage PromptTypeToMsgBoxIcon(Core.UI.PromptType t)
        {
            switch (t)
            {
                case Core.UI.PromptType.Error:
                    return MessageBoxImage.Error;
                case Core.UI.PromptType.Warning:
                    return MessageBoxImage.Warning;
                case Core.UI.PromptType.Information:
                    return MessageBoxImage.Information;
                default:
                    return MessageBoxImage.None;
            }
        }

        public static void CollectionShift<T>(int limit,  ObservableCollection<T> collection, T newObj)
        {
            if (collection.Contains(newObj))
            {
                collection.Remove(newObj);
            }
            collection.Insert(0, newObj);
            while (collection.Count > limit)
            {
                collection.RemoveAt(collection.Count-1);
            }
            return;
            T[] arr = new T[limit];
            arr[0] = newObj;
            for (int i = 0; i< collection.Count;  i++)
            {
                if((i)<limit-1)
                arr[i + 1] = collection[i];
            }
            collection.Clear();
            for (int i = 0; i < limit; i++)
            {
                if(arr[i]!=null)
                collection.Add(arr[i]);
            }
            
            
        }


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

       
    }
}
