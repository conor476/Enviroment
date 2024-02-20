using System.Collections.Generic;
using Enviroment.Models; // Assuming Category class is in this namespace

namespace Enviroment.ViewModels // This should match the folder structure
{
    public class GroupedCategory
    {
        public string GroupName { get; set; }
        public List<Category> Categories { get; set; }

        public GroupedCategory()
        {
            Categories = new List<Category>();
        }
    }
}
