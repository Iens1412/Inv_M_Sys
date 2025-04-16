using Inv_M_Sys.Models;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class ReportsService
    {
        public static async Task<List<Report>> GetAllReportsAsync()
        {
            var reports = new List<Report>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT Id, ReportTitle, ReportType, StartDate, EndDate, Status, Date FROM Reports ORDER BY Date DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    reports.Add(new Report
                    {
                        Id = reader.GetInt32(0),
                        ReportTitle = reader.GetString(1),
                        ReportType = reader.GetString(2),
                        StartDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        EndDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        Status = reader.GetString(5),
                        Date = reader.GetDateTime(6)
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch reports.");
                throw;
            }

            return reports;
        }

        public static async Task<List<Order>> GetReportDetailsAsync(int reportId)
        {
            var orders = new List<Order>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT OrderId, CustomerName, DeliveryDate, TotalPrice, Status FROM ReportDetails WHERE ReportId = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", reportId);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        CustomerName = reader.GetString(1),
                        DeliveryDate = reader.GetDateTime(2),
                        TotalPrice = reader.GetDecimal(3),
                        Status = Enum.Parse<OrderStatus>(reader.GetString(4))
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load report details.");
                throw;
            }

            return orders;
        }

        public static async Task DeleteReportAsync(int reportId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                using var cmd1 = new NpgsqlCommand("DELETE FROM ReportDetails WHERE ReportId = @Id", conn);
                cmd1.Parameters.AddWithValue("@Id", reportId);
                await cmd1.ExecuteNonQueryAsync();

                using var cmd2 = new NpgsqlCommand("DELETE FROM Reports WHERE Id = @Id", conn);
                cmd2.Parameters.AddWithValue("@Id", reportId);
                await cmd2.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete report #{ReportId}", reportId);
                throw;
            }
        }
    }
}