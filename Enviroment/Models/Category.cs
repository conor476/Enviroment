using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enviroment.Models
{
    public class Category
    {
        
        [Key]
        [Column("Category_id")] // Map this property to the 'Category_id' column in the database
        public int Category_id { get; set; }

        [Required]
        [StringLength(100)] // Adjust the max length as necessary for the category name
        public string Case_Name { get; set; }

        [StringLength(2550)] // Optional field, you can adjust the max length for the description
        public string Description { get; set; }

        public Category()
        {
            // Initialization of fields if necessary
            Case_Name = "";
            Description = "";
        }
    }
}

