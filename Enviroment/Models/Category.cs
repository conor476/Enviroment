using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enviroment.Models
{
    public class Category
    {
        
        [Key]
        [Column("Category_id")] 
        public int Category_id { get; set; }

        [Required]
        [StringLength(100)] 
        public string Case_Name { get; set; }

        [StringLength(2550)] 
        public string Description { get; set; }

        [StringLength(2550)] 
        public string Group { get; set; }

        [StringLength(2550)] 
        public string About { get; set; }

        public Category()
        {
           
            Case_Name = "";
            Description = "";
        }
    }
}

