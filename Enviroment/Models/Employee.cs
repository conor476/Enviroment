using System.ComponentModel.DataAnnotations;

namespace Enviroment.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required]
        [StringLength(50)]  // Assuming a max length of 50 characters for names
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]  // Assuming a max length of 50 characters for names
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]  // Assuming a max length of 100 characters for email
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(256)]  // Assuming a max length of 256 characters for hashed password
        public string Password { get; set; }


    }
}
