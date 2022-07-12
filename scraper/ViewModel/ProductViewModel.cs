using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using scraper.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;

namespace scraper.ViewModel
{
    public class ProductViewModel : BaseViewModel
    {
        public ProductViewModel(Product p)
        {
            Price = p.price;
            Title = p.title;
            UPC = p.upc;
            SKU = p.sku;
            Link = p.link;
            ImgUrl = p.imageUrl;
            ID = p.id;
            try
            {
                Website = new Uri(p.link).Host;
            }
            catch (Exception)
            {
                Debug.WriteLine("no uri " + Link);
                
            }
            
        }
        private double _Price;
        public double Price
        {
            set { _Price = value; notif(nameof(Price)); }
            get { return _Price; }
        }

        private string _ID;
        public string ID
        {
            set { _ID = value; notif(nameof(ID)); }
            get { return _ID; }
        }

        private string _UPC;
        public string UPC
        {
            set { _UPC = value; notif(nameof(UPC)); }
            get { return _UPC; }
        }


        private string _SKU;
        public string SKU
        {
            set { _SKU = value; notif(nameof(SKU)); }
            get { return _SKU; }
        }


        private string _Website;
        public string Website
        {
            set { _Website = value; notif(nameof(Website)); }
            get { return _Website; }
        }


        private string _Link;
        public string Link
        {
            set { _Link = value; notif(nameof(Link)); }
            get { return _Link; }
        }

        private string _Title;
        public string Title
        {
            set { _Title = value; notif(nameof(Title)); }
            get { return _Title; }
        }

        private string _ImgUrl;
        public string ImgUrl
        {
            set { _ImgUrl = value; notif(nameof(ImgUrl)); notif(nameof(ImgSrc)); }
            get { return _ImgUrl; }
        }

        public ImageSource ImgSrc
        {
            
            get { return new BitmapImage(new Uri(@"E:\TOOLS\scraper\scraper\scripts\" + ImgUrl)); }
        }


        private Rating _Rating;
        public Rating Rating
        {
            set { _Rating = value; notif(nameof(Rating)); }
            get { return _Rating; }
        }

        private void hndlCopyToClipCommand(string propToCopy)
        {
            switch (propToCopy)
            {
                case "link": Clipboard.SetText(this.Link); break;

                case "upc": Clipboard.SetText(this.UPC); break;

                case "sku": Clipboard.SetText(this.SKU); break;
                default:
                    break;
            }
        }
        public ICommand CopyToClipCommand
        {
            get
            {
                return new Mi.Common.MICommand<string>(hndlCopyToClipCommand);
            }
        }

    }
}
