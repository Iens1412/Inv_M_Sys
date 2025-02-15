using System.ComponentModel.DataAnnotations;

namespace Inv_M_Sys.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; } // Primary Key

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Address { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}