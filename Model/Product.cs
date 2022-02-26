using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopbridgeEventProcessor.Models
{
    public class Product
    {
        [Key]
        public int Product_Id { get; set; }
        
        [Required]
        public string ProductUpc { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }

        public int Stock { get; set; }
    }
}
