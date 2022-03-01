using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopbridgeEventProcessor.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("uuid-ossp");
            builder.Entity<Product>()
                .HasIndex(p => p.ProductUpc)
                .IsUnique();
        }

        public DbSet<Product> Product { get; set; }
    }
}
