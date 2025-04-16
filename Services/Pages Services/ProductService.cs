using Dapper;
using Inv_M_Sys.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class ProductService
    {
        // Fetch all products with category details
        public static async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT p.Id, p.ProductName, p.CategoryId, c.Name as CategoryName,
                       p.Quantity, p.Price, p.MinQuantity, p.Supplier, p.Description
                FROM Products p
                JOIN Categories c ON p.CategoryId = c.Id";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CategoryId = reader.GetInt32(2),
                    CategoryName = reader.GetString(3),
                    Category = new Category
                    {
                        CatID = reader.GetInt32(2),
                        CategoryName = reader.GetString(3)
                    },
                    Quantity = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Price = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    MinQuantity = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    Supplier = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Description = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }

            return products;
        }

        // Fetch all categories
        public static async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT Id, Name FROM Categories", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CatID = reader.GetInt32(0),
                    CategoryName = reader.GetString(1)
                });
            }

            return categories;
        }

        // Add a new product
        public static async Task AddProductAsync(Product product)
        {
            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            var exists = await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM Products WHERE ProductName = @name AND CategoryId = @categoryId)",
                new { name = product.Name, categoryId = product.CategoryId });

            if (exists)
            {
                throw new Exception("A product with this name already exists in the selected category.");
            }

            var query = @"
                INSERT INTO Products (ProductName, CategoryId, Quantity, Price, MinQuantity, Supplier, Description)
                VALUES (@Name, @CategoryId, @Quantity, @Price, @MinQuantity, @Supplier, @Description)";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity ?? 0);
            cmd.Parameters.AddWithValue("@Price", product.Price ?? 0);
            cmd.Parameters.AddWithValue("@MinQuantity", product.MinQuantity ?? 0);
            cmd.Parameters.AddWithValue("@Supplier", (object?)product.Supplier ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        // Update an existing product
        public static async Task UpdateProductAsync(Product product)
        {
            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            var exists = await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM Products WHERE ProductName = @name AND CategoryId = @categoryId AND Id != @id)",
                new { name = product.Name, categoryId = product.CategoryId, id = product.Id });

            if (exists)
            {
                throw new Exception("Another product with this name already exists in the selected category.");
            }

            var query = @"
                UPDATE Products
                SET ProductName = @Name,
                    CategoryId = @CategoryId,
                    Quantity = @Quantity,
                    Price = @Price,
                    MinQuantity = @MinQuantity,
                    Supplier = @Supplier,
                    Description = @Description
                WHERE Id = @Id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", product.Id);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity ?? 0);
            cmd.Parameters.AddWithValue("@Price", product.Price ?? 0);
            cmd.Parameters.AddWithValue("@MinQuantity", product.MinQuantity ?? 0);
            cmd.Parameters.AddWithValue("@Supplier", (object?)product.Supplier ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        // Delete a product by ID
        public static async Task DeleteProductAsync(int productId)
        {
            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("DELETE FROM Products WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", productId);

            await cmd.ExecuteNonQueryAsync();
        }

        // Optional: Filter products by category
        public static async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
        {
            var products = new List<Product>();

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT Id, ProductName, Price, Quantity FROM Products WHERE CategoryId = @CategoryId", conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Quantity = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                });
            }

            return products;
        }
    }
}
