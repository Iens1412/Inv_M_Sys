using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Inv_M_Sys.Models
{
    public class InventoryContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseNpgsql("Host=172.18.0.2;Port=5432;Database=DevDB;Username=dev_user;Password=DevPassword123");
            optionsBuilder.UseSqlite("Data Source=Services/DevDB.db");
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)  // A Product has one Category
                .WithMany(c => c.Products)  // A Category has many Products
                .HasForeignKey(p => p.CategoryID)  // Foreign key in Product
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete
        }

    }
}
