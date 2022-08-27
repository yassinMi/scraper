using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Converters
{

    /// <summary>
   
    /// </summary>
    [ValueConversion(typeof(scraper.Core.UI.PromptType),typeof(MahApps.Metro.IconPacks.PackIconFontAwesomeKind))]
    public class PromptTypeToFontAwesomeIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(! (value is scraper.Core.UI.PromptType))
            {
                return PackIconFontAwesomeKind.ExclamationTriangleSolid;
                
            }
            scraper.Core.UI.PromptType t = (scraper.Core.UI.PromptType) value;
            switch (t)
            {
                case scraper.Core.UI.PromptType.Error:
                    return PackIconFontAwesomeKind.SadTearSolid;
                case scraper.Core.UI.PromptType.Warning:
                    return PackIconFontAwesomeKind.ExclamationTriangleSolid;
                case scraper.Core.UI.PromptType.Information:
                    return PackIconFontAwesomeKind.InfoCircleSolid;
                case scraper.Core.UI.PromptType.Question:
                    return PackIconFontAwesomeKind.QuestionSolid;
                default:
                    return PackIconFontAwesomeKind.ExclamationTriangleSolid;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    


}
