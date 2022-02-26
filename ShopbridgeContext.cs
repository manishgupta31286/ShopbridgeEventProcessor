using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopbridgeEventProcessor.Models;


namespace ShopbridgeEventProcessor
{
    public class ShopbridgeContext : DbContext
    {
        public ShopbridgeContext(DbContextOptions<ShopbridgeContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>()
                .HasIndex(p => p.ProductUpc)
                .IsUnique();
        }

        public DbSet<Product> Product { get; set; }
    }
}
