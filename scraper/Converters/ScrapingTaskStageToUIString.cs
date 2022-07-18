using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using scraper.Model;
using scraper.Core;

namespace Converters
{
   
    /// <summary>
    /// more user friendly names
    /// </summary>
    [ValueConversion(typeof(Enum),typeof(string))]
    public class ScrapingTaskStageToUIString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!((value is Enum  )))
            {
                return "err";
            }
            ScrapTaskStage? stage = value as ScrapTaskStage?;
            if (stage.HasValue == false)
            {
                return "err";
            }
            switch (stage)
            {
                case ScrapTaskStage.Ready: return "";
                case ScrapTaskStage.DownloadingData: return "Dwonloading";
                case ScrapTaskStage.Paused: return "Paused";
                case ScrapTaskStage.ConvertingData: return "Converting";
                case ScrapTaskStage.Success: return "Completed";
                case ScrapTaskStage.Failed: return "Failed";
                default: return "";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
  

}
