using System;

namespace Inv_M_Sys.Models
{
    public class Customer
    {
        public int Id { get; set; } // Primary Key
        public string CompanyName { get; set; }
        public string FirstName { get; set; } // Required
        public string LastName { get; set; } // Required
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        public Customer() { }

        public Customer(int id, string companyName, string firstName, string lastName, string email, string phoneNumber, string address, string notes)
        {
            Id = id;
            CompanyName = companyName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Notes = notes;
        }


        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }

        // Override ToString() for debugging
        public override string ToString()
        {
            return $"{FirstName} {LastName} - {CompanyName}";
        }
    }
}