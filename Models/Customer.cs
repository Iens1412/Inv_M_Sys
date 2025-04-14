using System;
using System.ComponentModel;

namespace Inv_M_Sys.Models
{
    public class Customer : INotifyPropertyChanged
    {
        private int _id;
        private string _companyName;
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phoneNumber;
        private string _address;
        private string _notes;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(nameof(CompanyName)); }
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public string FullName => $"{FirstName} {LastName}";

        public override string ToString()
        {
            return $"{FirstName} {LastName} - {CompanyName}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}