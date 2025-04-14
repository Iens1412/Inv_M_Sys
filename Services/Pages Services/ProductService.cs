using Inv_M_Sys.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class ProductService
    {
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
                    Price = reader.GetDecimal(2),
                    Quantity = reader.GetInt32(3)
                });
            }
            return products;
        }
    }
}
