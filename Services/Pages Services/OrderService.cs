using Inv_M_Sys.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inv_M_Sys.Services.Pages_Services
{
    public static class OrderService
    {
        public static async Task<int> PlaceOrderAsync(Order order, List<OrderItem> items)
        {
            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var orderCmd = new NpgsqlCommand(
                    "INSERT INTO Orders (CustomerId, DeliveryDate, TotalPrice, Status) VALUES (@CustomerId, @DeliveryDate, @TotalPrice, @Status) RETURNING Id", conn);
                orderCmd.Parameters.AddWithValue("@CustomerId", order.Id);
                orderCmd.Parameters.AddWithValue("@DeliveryDate", order.DeliveryDate);
                orderCmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
                orderCmd.Parameters.AddWithValue("@Status", order.Status.ToString());

                int orderId = (int)await orderCmd.ExecuteScalarAsync();

                foreach (var item in items)
                {
                    var itemCmd = new NpgsqlCommand(
                        "INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, TotalPrice) VALUES (@OrderId, @ProductId, @Quantity, @Price, @TotalPrice)", conn);
                    itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.Product.Id);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@Price", item.Product.Price);
                    itemCmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                    await itemCmd.ExecuteNonQueryAsync();

                    var updateCmd = new NpgsqlCommand("UPDATE Products SET Quantity = Quantity - @Qty WHERE Id = @Id", conn);
                    updateCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                    updateCmd.Parameters.AddWithValue("@Id", item.Product.Id);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return orderId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
