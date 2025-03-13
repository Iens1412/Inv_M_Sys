using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inv_M_Sys.Models
{
    public class Product
    {
        public int Id { get; set; }  // Corrected the name to ProductID
        public string Name { get; set; }
        public int CategoryId { get; set; } // Foreign key (referencing Category)
        public string CategoryName { get; set; } // For displaying CategoryName
        public Category Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int MinQuantity { get; set; }
        public string Supplier { get; set; }
        public string Description { get; set; }
    }
}