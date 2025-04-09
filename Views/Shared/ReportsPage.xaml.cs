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
using Microsoft.Win32;

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

        // lOG OUT Button logic
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        //Close Button Logic
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        //Minimize Button Logic
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        //Home Button Logic
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        }
        #endregion

        #region ControlButtons

        //Home Button Logic
        private void ExpBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportReportContainer.Visibility = Visibility.Visible;
        }

        //Home Button Logic
        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateContainer.Visibility = Visibility.Visible;
        }

        //Home Button Logic
        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteSavedReport_Click(sender, e);
        }
        #endregion

        #region Container Buttons
        //Generate Button Logic with fucntion in it
        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            string title = ReportTitleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a title for the report.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content?.ToString() ?? "All";
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;

            List<Order> filtered = new();

            try
            {
                Log.Information("Generating new report with Title: {Title}, Status: {Status}, Start: {Start}, End: {End}", title, status, start, end);

                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                // Build SQL query with dynamic filters
                string query = @"
                    SELECT o.Id, c.FirstName || ' ' || c.LastName AS CustomerName,
                        o.DeliveryDate, o.TotalPrice, o.Status
                    FROM Orders o
                    JOIN Customers c ON o.CustomerId = c.Id
                    WHERE 1=1"; // Simplifies AND chaining

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

                if (filtered.Count == 0)
                {
                    Log.Warning("No orders matched the filter. Report generation aborted.");
                    MessageBox.Show("No matching orders found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                reader.Close();

                // Insert report metadata
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

                // Insert each order as part of the report
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
                await LoadPreviousReportsAsync();

                GenerateContainer.Visibility = Visibility.Collapsed;

                Log.Information("Sales report '{Title}' generated with {Count} orders", title, filtered.Count);
                MessageBox.Show("Sales report successfully generated and saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during report generation.");
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //GenBack Button Logic
        private void GenBack_Click(object sender, RoutedEventArgs e)
        {
            GenerateContainer.Visibility = Visibility.Collapsed;
            SalesListView.ItemsSource = null;
            SelectedReportTitle.Text = "No Report Selected";
            SelectedReportDetails.Text = "";
        }

        //ExpBack Button Logic
        private void ExpBack_Click(object sender, RoutedEventArgs e)
        {
            ExportReportContainer.Visibility = Visibility.Collapsed;
        }

        //ExportAsCSV Button Logic function included
        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var writer = new StreamWriter(dialog.FileName);
                    using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                    });

                    csv.WriteField("Order ID");
                    csv.WriteField("Customer Name");
                    csv.WriteField("Delivery Date");
                    csv.WriteField("Total Price");
                    csv.WriteField("Status");
                    csv.NextRecord();

                    foreach (var order in SalesList)
                    {
                        csv.WriteField(order.Id);
                        csv.WriteField(order.CustomerName);
                        csv.WriteField(order.DeliveryDate.ToString("yyyy-MM-dd"));
                        csv.WriteField(order.TotalPrice.ToString("C", CultureInfo.CurrentCulture));
                        csv.WriteField(order.Status.ToString());
                        csv.NextRecord();
                    }

                    MessageBox.Show("CSV report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "CSV export failed.");
                    MessageBox.Show("CSV export failed: " + ex.Message);
                }
            }
        }

        //ExportAsExcel Button Logic function included
        private void ExportAsExcel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Sales Report");

                    worksheet.Cell("A1").Value = "Sales Report";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(3, 1).Value = "Order ID";
                    worksheet.Cell(3, 2).Value = "Customer Name";
                    worksheet.Cell(3, 3).Value = "Delivery Date";
                    worksheet.Cell(3, 4).Value = "Total Price";
                    worksheet.Cell(3, 5).Value = "Status";
                    worksheet.Range("A3:E3").Style.Font.Bold = true;

                    for (int i = 0; i < SalesList.Count; i++)
                    {
                        var order = SalesList[i];
                        worksheet.Cell(i + 4, 1).Value = order.Id;
                        worksheet.Cell(i + 4, 2).Value = order.CustomerName;
                        worksheet.Cell(i + 4, 3).Value = order.DeliveryDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(i + 4, 4).Value = order.TotalPrice;
                        worksheet.Cell(i + 4, 5).Value = order.Status.ToString();
                    }

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(dialog.FileName);
                    MessageBox.Show("Excel report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Excel export failed.");
                    MessageBox.Show("Excel export failed: " + ex.Message);
                }
            }
        }

        //ExportAsPDF Button Logic function included
        private void ExportAsPDF_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PdfDocument doc = new PdfDocument();
                    doc.Info.Title = "Sales Report";
                    PdfPage page = doc.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont headerFont = new XFont("Verdana", 14, XFontStyle.Bold);
                    XFont font = new XFont("Verdana", 10);
                    double y = 40;

                    gfx.DrawString("Sales Report", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
                    y += 40;

                    gfx.DrawString("Generated On: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), font, XBrushes.Gray, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
                    y += 30;

                    foreach (var order in SalesList)
                    {
                        string line = $"#{order.Id} - {order.CustomerName} - {order.DeliveryDate:yyyy-MM-dd} - {order.TotalPrice:C} - {order.Status}";
                        gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
                        y += 20;

                        if (y > page.Height - 60)
                        {
                            page = doc.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = 40;
                        }
                    }

                    doc.Save(dialog.FileName);
                    MessageBox.Show("PDF report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PDF export failed.");
                    MessageBox.Show("PDF export failed: " + ex.Message);
                }
            }
        }

        #endregion

        #region Helpers

        //ReportsPage_Loaded function
        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("ReportsPage loaded. Loading saved reports.");
            await LoadPreviousReportsAsync();
        }

        //LoadPreviousReportsAsync loading saved reports from database
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

        //load info when selceting report
        private async void ReportsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportsListView.SelectedItem is Report selectedReport)
            {
                await LoadReportDetailsAsync(selectedReport.Id);
                SelectedReportTitle.Text = selectedReport.ReportTitle;
                SelectedReportDetails.Text = $"From {selectedReport.StartDate:yyyy-MM-dd} to {selectedReport.EndDate:yyyy-MM-dd} ({selectedReport.Status})";
            }
        }

        //load the details of the selected report
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

                Log.Information("Report #{ReportId} loaded with {Count} orders", reportId, SalesList.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load details for report #{ReportId}", reportId);
                MessageBox.Show("Failed to load report details: " + ex.Message);
            }
        }

        //delete the report
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

                    // Delete details first (FK)
                    using var cmd1 = new NpgsqlCommand("DELETE FROM ReportDetails WHERE ReportId = @Id", conn);
                    cmd1.Parameters.AddWithValue("@Id", selectedReport.Id);
                    await cmd1.ExecuteNonQueryAsync();

                    // Then delete report metadata
                    using var cmd2 = new NpgsqlCommand("DELETE FROM Reports WHERE Id = @Id", conn);
                    cmd2.Parameters.AddWithValue("@Id", selectedReport.Id);
                    await cmd2.ExecuteNonQueryAsync();

                    await LoadPreviousReportsAsync();
                    SalesListView.ItemsSource = null;
                    SelectedReportTitle.Text = "No Report Selected";
                    SelectedReportDetails.Text = "";

                    Log.Information("Report #{ReportId} deleted", selectedReport.Id);
                    MessageBox.Show("Report deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to delete report #{ReportId}", selectedReport.Id);
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

        //search for specific report
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

        //reload the page
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPreviousReportsAsync();
            SalesListView.ItemsSource = null;
        }
        #endregion
    }
}
