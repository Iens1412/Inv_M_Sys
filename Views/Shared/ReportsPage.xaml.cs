using CsvHelper;
using CsvHelper.Configuration;
using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace Inv_M_Sys.Views.Shared
{
    public partial class ReportsPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Order> SalesList = new();
        private ObservableCollection<Report> PreviousReportsList = new();

        public ReportsPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            Loaded += ReportsPage_Loaded;
        }

        #region Top Menu
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        }
        #endregion

        #region ControlButtons
        private void ExpBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportReportContainer.Visibility = Visibility.Visible;
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateContainer.Visibility = Visibility.Visible;
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteSavedReport_Click(sender, e);
        }
        #endregion

        #region Container Buttons
        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            string title = ((ComboBoxItem)ReportTypeComboBox.SelectedItem)?.Content?.ToString() ?? "Sales Report";
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content?.ToString() ?? "All";
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;

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
            WHERE o.IsDeleted = FALSE";

                if (status != "All")
                {
                    query += " AND o.Status = @Status";
                }

                if (start.HasValue)
                {
                    query += " AND o.DeliveryDate >= @Start";
                }

                if (end.HasValue)
                {
                    query += " AND o.DeliveryDate <= @End";
                }

                using var cmd = new NpgsqlCommand(query, conn);

                if (status != "All")
                    cmd.Parameters.AddWithValue("@Status", status);

                if (start.HasValue)
                    cmd.Parameters.AddWithValue("@Start", start.Value);

                if (end.HasValue)
                    cmd.Parameters.AddWithValue("@End", end.Value);

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

                if (filtered.Count == 0)
                {
                    MessageBox.Show("No matching orders found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                reader.Close();

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

                foreach (var order in filtered)
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

                await LoadPreviousReportsAsync(); // Refresh reports list
                MessageBox.Show("Sales report successfully generated and saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate and save report.");
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenBack_Click(object sender, RoutedEventArgs e)
        {
            GenerateContainer.Visibility = Visibility.Collapsed;
            SalesListView.ItemsSource = null;
            SelectedReportTitle.Text = "No Report Selected";
            SelectedReportDetails.Text = "";
        }

        private void ExpBack_Click(object sender, RoutedEventArgs e)
        {
            ExportReportContainer.Visibility = Visibility.Collapsed;
        }

        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            // Export logic...
        }

        private void ExportAsExcel_Click(object sender, RoutedEventArgs e)
        {
            // Export logic...
        }

        private void ExportAsPDF_Click(object sender, RoutedEventArgs e)
        {
            // Export logic...
        }
        #endregion

        #region Helpers
        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPreviousReportsAsync();
        }

        private async Task LoadPreviousReportsAsync()
        {
            PreviousReportsList.Clear();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT Id, ReportTitle, ReportType, StartDate, EndDate, Status, Date FROM Reports ORDER BY Date DESC";

                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    PreviousReportsList.Add(new Report
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

                ReportsListView.ItemsSource = PreviousReportsList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load saved reports.");
                MessageBox.Show("Error loading saved reports: " + ex.Message);
            }
        }

        private async void ReportsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportsListView.SelectedItem is Report selectedReport)
            {
                await LoadReportDetailsAsync(selectedReport.Id);
                SelectedReportTitle.Text = selectedReport.ReportTitle;
                SelectedReportDetails.Text = $"From {selectedReport.StartDate:yyyy-MM-dd} to {selectedReport.EndDate:yyyy-MM-dd} ({selectedReport.Status})";
            }
        }

        private async Task LoadReportDetailsAsync(int reportId)
        {
            try
            {
                SalesList.Clear();
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                string query = "SELECT OrderId, CustomerName, DeliveryDate, TotalPrice, Status FROM ReportDetails WHERE ReportId = @Id";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", reportId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    SalesList.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        CustomerName = reader.GetString(1),
                        DeliveryDate = reader.GetDateTime(2),
                        TotalPrice = reader.GetDecimal(3),
                        Status = Enum.Parse<OrderStatus>(reader.GetString(4))
                    });
                }

                SalesListView.ItemsSource = SalesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load report details: " + ex.Message);
            }
        }

        private async void DeleteSavedReport_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsListView.SelectedItem is Report selectedReport)
            {
                var confirm = MessageBox.Show("Are you sure you want to delete this report?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    await conn.OpenAsync();

                    using var cmd1 = new NpgsqlCommand("DELETE FROM ReportDetails WHERE ReportId = @Id", conn);
                    cmd1.Parameters.AddWithValue("@Id", selectedReport.Id);
                    await cmd1.ExecuteNonQueryAsync();

                    using var cmd2 = new NpgsqlCommand("DELETE FROM Reports WHERE Id = @Id", conn);
                    cmd2.Parameters.AddWithValue("@Id", selectedReport.Id);
                    await cmd2.ExecuteNonQueryAsync();

                    await LoadPreviousReportsAsync();
                    SalesListView.ItemsSource = null;
                    SelectedReportTitle.Text = "No Report Selected";
                    SelectedReportDetails.Text = "";

                    MessageBox.Show("Report deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete report: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a report to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Search and refresh
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = RoundedTextBox.Text.ToLower();
            var filtered = SalesList.Where(o =>
                o.Id.ToString().Contains(query) ||
                o.CustomerName.ToLower().Contains(query) ||
                o.DeliveryDate.ToString("yyyy-MM-dd").Contains(query) ||
                o.Status.ToString().ToLower().Contains(query));

            SalesListView.ItemsSource = new ObservableCollection<Order>(filtered);
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPreviousReportsAsync();
            SalesListView.ItemsSource = null;
        }
        #endregion
    }
}
