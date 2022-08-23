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
using scraper.Core.Workspace;
using scraper.Core;

namespace scraper.ViewModel
{
    public class ElementViewModel : BaseViewModel
    {

        /// <summary>
        /// NOTE: datagrid view cells are bound to the Model. while listVew UI fields are bound to the properties exposed directly by this ViewModel
        /// the listView props are mapped based on the elementDescriptor roles information (the only reason why it's required)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="elementDescriptor"></param>
        public ElementViewModel(dynamic p,ElementDescription elementDescriptor)
        {
            Model = p;
            //Name, address,Phonenumber,MobilePhonenumberEmail, Employees,deligation, etc.
            // Name = p.company;
            // Description = p.description;
            /* PhoneNumber = p.phonenumber;
             Email = p.email;
             Link = p.link;
             ImgUrl = p.imageUrl;
             Employees = p.employees;
             Address = p.address;
             Website = p.website;
             ContactPerson = p.contactPerson;
             Year = p.year;*/



            
            Name = tryGetValueByRole(Model, elementDescriptor, FieldRole.Title);
            ImgUrl = tryGetValueByRole(Model, elementDescriptor, FieldRole.Thumbnail);
            


        }
        /// <summary>
        /// returns the value of the field of which the role matches the specified role, using the property name - role mapping provided by the elementDescriptor
        /// </summary>
        /// <returns>null if no property matches the role.</returns>
        object tryGetValueByRole(object model, ElementDescription elementDescriptor, FieldRole role)
        {
            var targetField = elementDescriptor.Fields.FirstOrDefault(f => f.Role == FieldRole.Title);
            if (targetField != null) return model.GetType().GetProperty(targetField.Name)?.GetValue(model);
            return null;
        }
        public dynamic Model { get; set; }

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


        private string _ContactPerson;
        public string ContactPerson
        {
            set { _ContactPerson = value; notif(nameof(ContactPerson)); }
            get { return _ContactPerson; }
        }


        private string _Year;
        public string Year
        {
            set { _Year = value; notif(nameof(Year)); }
            get { return _Year; }
        }




        public ImageSource ImgSrc
        {
            
            get {
                if (string.IsNullOrWhiteSpace(ImgUrl)) return null;
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
            try
            {
                switch (propToCopy)
                {
                    case "link": Clipboard.SetDataObject(this.Link); break;

                    case "phone": Clipboard.SetDataObject(this.PhoneNumber); break;

                    case "email": Clipboard.SetDataObject(this.Email); break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

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
