using Inv_M_Sys.Models;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class CustomerService
    {
        public static async Task<List<Customer>> GetAllAsync()
        {
            var customers = new List<Customer>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Customers", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(ReadCustomer(reader));
                    }
                }
            }
            return customers;
        }

        public static async Task<Customer?> GetByIdAsync(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Customers WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return ReadCustomer(reader);
                        }
                    }
                }
            }
            return null;
        }

        public static async Task<bool> ExistsAsync(Customer customer)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT COUNT(*) FROM Customers 
                    WHERE 
                        CompanyName = @CompanyName AND 
                        FirstName = @FirstName AND 
                        LastName = @LastName AND 
                        Email = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);

                    int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public static async Task AddAsync(Customer customer)
        {
            if (await ExistsAsync(customer))
                throw new Exception("Customer already exists.");

            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO Customers (CompanyName, FirstName, LastName, Email, PhoneNumber, Address, Notes) 
                    VALUES (@CompanyName, @FirstName, @LastName, @Email, @PhoneNumber, @Address, @Notes)", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Address", customer.Address);
                    cmd.Parameters.AddWithValue("@Notes", customer.Notes);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UpdateAsync(Customer customer)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE Customers SET 
                        CompanyName = @CompanyName, 
                        FirstName = @FirstName, 
                        LastName = @LastName, 
                        Email = @Email, 
                        PhoneNumber = @PhoneNumber, 
                        Address = @Address, 
                        Notes = @Notes 
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Address", customer.Address);
                    cmd.Parameters.AddWithValue("@Notes", customer.Notes);
                    cmd.Parameters.AddWithValue("@Id", customer.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAsync(int customerId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM Customers WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", customerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Helper method to read a customer object
        private static Customer ReadCustomer(NpgsqlDataReader reader)
        {
            return new Customer
            {
                Id = reader.GetInt32(0),
                CompanyName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                PhoneNumber = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Notes = reader.IsDBNull(7) ? "" : reader.GetString(7)
            };
        }
    }
}
