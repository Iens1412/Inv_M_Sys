using Inv_M_Sys.Models;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class ReportGenerationService
    {
        public static async Task<(int ReportId, List<Order> Orders)> GenerateReportAsync(string title, string status, DateTime? start, DateTime? end)
        {
            List<Order> filtered = new();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                string query = @"
                    SELECT o.Id, c.FirstName || ' ' || c.LastName AS CustomerName,
                        o.DeliveryDate, o.TotalPrice, o.Status
                    FROM Orders o
                    JOIN Customers c ON o.CustomerId = c.Id
                    WHERE 1=1";

                if (status != "All") query += " AND o.Status = @Status";
                if (start.HasValue) query += " AND o.DeliveryDate >= @Start";
                if (end.HasValue) query += " AND o.DeliveryDate <= @End";

                using var cmd = new NpgsqlCommand(query, conn);
                if (status != "All") cmd.Parameters.AddWithValue("@Status", status);
                if (start.HasValue) cmd.Parameters.AddWithValue("@Start", start.Value);
                if (end.HasValue) cmd.Parameters.AddWithValue("@End", end.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    filtered.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        CustomerName = reader.GetString(1),
                        DeliveryDate = reader.GetDateTime(2),
                        TotalPrice = reader.GetDecimal(3),
                        Status = Enum.Parse<OrderStatus>(reader.GetString(4))
                    });
                }

                reader.Close();

                if (filtered.Count == 0)
                    return (-1, filtered);

                using var transaction = conn.BeginTransaction();
                string insertReportQuery = @"
                    INSERT INTO Reports (ReportTitle, ReportType, Details, StartDate, EndDate, Status)
                    VALUES (@Title, @Type, @Details, @Start, @End, @Status)
                    RETURNING Id;";

                using var insertReportCmd = new NpgsqlCommand(insertReportQuery, conn);
                insertReportCmd.Parameters.AddWithValue("@Title", title);
                insertReportCmd.Parameters.AddWithValue("@Type", "Sales Report");
                insertReportCmd.Parameters.AddWithValue("@Details", $"{filtered.Count} orders included.");
                insertReportCmd.Parameters.AddWithValue("@Start", (object?)start ?? DBNull.Value);
                insertReportCmd.Parameters.AddWithValue("@End", (object?)end ?? DBNull.Value);
                insertReportCmd.Parameters.AddWithValue("@Status", status);

                int reportId = Convert.ToInt32(await insertReportCmd.ExecuteScalarAsync());

                foreach (var order in filtered.OrderBy(o => o.Id))
                {
                    string insertDetailQuery = @"
                        INSERT INTO ReportDetails (ReportId, OrderId, CustomerName, DeliveryDate, TotalPrice, Status)
                        VALUES (@ReportId, @OrderId, @CustomerName, @DeliveryDate, @TotalPrice, @Status);";

                    using var insertDetailCmd = new NpgsqlCommand(insertDetailQuery, conn);
                    insertDetailCmd.Parameters.AddWithValue("@ReportId", reportId);
                    insertDetailCmd.Parameters.AddWithValue("@OrderId", order.Id);
                    insertDetailCmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    insertDetailCmd.Parameters.AddWithValue("@DeliveryDate", order.DeliveryDate);
                    insertDetailCmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
                    insertDetailCmd.Parameters.AddWithValue("@Status", order.Status.ToString());

                    await insertDetailCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return (reportId, filtered);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating report with title '{Title}'", title);
                throw;
            }
        }
    }
}
