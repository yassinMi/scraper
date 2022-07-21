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
    public class BusinessViewModel : BaseViewModel
    {
        public BusinessViewModel(Business p)
        {
            //Name, address,Phonenumber,MobilePhonenumberEmail, Employees,deligation, etc.
            Name = p.name;
            Description = p.description;
            PhoneNumber = p.phonenumber;
            Email = p.email;
            Link = p.link;
            ImgUrl = p.imageUrl;
            ID =  p.ID;
            Employees = p.employees;
            Address = p.address;
            Website = p.website;
            
            
        }
        private string _Name;
        public string Name
        {
            set { _Name = value; notif(nameof(Name)); }
            get { return _Name; }
        }

        private IElementID _ID;
        public IElementID ID
        {
            set { _ID = value; notif(nameof(ID)); }
            get { return _ID; }
        }

        private string _PhoneNumber;
        public string PhoneNumber
        {
            set { _PhoneNumber = value; notif(nameof(PhoneNumber)); }
            get { return _PhoneNumber; }
        }


        private string _Address;
        public string Address
        {
            set { _Address = value; notif(nameof(Address)); }
            get { return _Address; }
        }



        private string _Email;
        public string Email
        {
            set { _Email = value; notif(nameof(Email)); }
            get { return _Email; }
        }

        private string _Employees;
        public string Employees
        {
            set { _Employees = value; notif(nameof(Employees)); }
            get { return _Employees; }
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

        private string _Description;
        public string Description
        {
            set { _Description = value; notif(nameof(Description)); }
            get { return _Description; }
        }

        private string _ImgUrl;
        public string ImgUrl
        {
            set { _ImgUrl = value; notif(nameof(ImgUrl)); notif(nameof(ImgSrc)); }
            get { return _ImgUrl; }
        }

        public ImageSource ImgSrc
        {
            
            get {
                if(File.Exists(Path.Combine(Workspace.Current.Directory, ImgUrl))){
                    return new BitmapImage(new Uri(Path.Combine(Workspace.Current.Directory, ImgUrl)));
                }
                else
                {
                    return null;
                }
            }
        }


      

        private void hndlCopyToClipCommand(string propToCopy)
        {
            switch (propToCopy)
            {
                case "link": Clipboard.SetText(this.Link); break;

                case "phone": Clipboard.SetText(this.PhoneNumber); break;

                case "email": Clipboard.SetText(this.Email); break;
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
