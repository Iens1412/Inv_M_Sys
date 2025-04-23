using Inv_M_Sys.Models;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class UserService
    {
        public static async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM Users", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                UserID = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                Phone = reader.GetString(4),
                                Username = reader.GetString(5),
                                HashedPassword = reader.GetString(6),
                                Address = reader.GetString(7),
                                Role = Enum.TryParse<UserRole>(reader.GetString(8), out var role) ? role : UserRole.SellingStaff
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading users.");
            }
            return users;
        }

        public static async Task AddAsync(User user)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, Role, Username, Password) 
                        VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @Role, @Username, @Password)", conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", user.LastName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Phone", user.Phone);
                        cmd.Parameters.AddWithValue("@Address", user.Address);
                        cmd.Parameters.AddWithValue("@Role", user.Role.ToString());
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@Password", user.HashedPassword);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding user.");
                throw;
            }
        }

        public static async Task UpdateAsync(User user)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE Users SET 
                            FirstName = @FirstName,
                            LastName = @LastName,
                            Email = @Email,
                            PhoneNumber = @Phone,
                            Address = @Address,
                            Role = @Role,
                            Username = @Username,
                            Password = @Password
                        WHERE Id = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", user.LastName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Phone", user.Phone);
                        cmd.Parameters.AddWithValue("@Address", user.Address);
                        cmd.Parameters.AddWithValue("@Role", user.Role.ToString());
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@Password", user.HashedPassword);
                        cmd.Parameters.AddWithValue("@UserID", user.UserID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating user.");
                throw;
            }
        }

        public static async Task DeleteAsync(int userId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("DELETE FROM Users WHERE Id = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting user.");
                throw;
            }
        }

        public static async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    if (excludeUserId.HasValue)
                        query += " AND Id != @UserId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        if (excludeUserId.HasValue)
                            cmd.Parameters.AddWithValue("@UserId", excludeUserId.Value);

                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking username existence.");
                return true;
            }
        }
    }
}
