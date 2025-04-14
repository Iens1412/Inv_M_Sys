using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inv_M_Sys.Models;
using Serilog;

namespace Inv_M_Sys.Services
{
    public static class CategoryService
    {
        //Handel Databse ORDERS
        public static async Task<List<Category>> GetAllAsync()
        {
            var categories = new List<Category>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT * FROM Categories", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    categories.Add(new Category
                    {
                        CatID = reader.GetInt32(0),
                        CategoryName = reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving categories from database.");
                throw;
            }
            return categories;
        }

        public static async Task<bool> ExistsAsync(string name)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Categories WHERE LOWER(Name) = LOWER(@Name)", conn);
                cmd.Parameters.AddWithValue("@Name", name);
                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking if category '{CategoryName}' exists.", name);
                throw;
            }
        }

        public static async Task AddAsync(string name)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("INSERT INTO Categories (Name) VALUES (@Name)", conn);
                cmd.Parameters.AddWithValue("@Name", name);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding category: {CategoryName}", name);
                throw;
            }
        }

        public static async Task UpdateAsync(int id, string newName)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("UPDATE Categories SET Name = @Name WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Name", newName);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating category ID {CategoryId} to name '{NewName}'", id, newName);
                throw;
            }
        }

        public static async Task DeleteAsync(int id)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("DELETE FROM Categories WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting category ID {CategoryId}", id);
                throw;
            }
        }
    }
}
