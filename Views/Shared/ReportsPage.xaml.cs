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
using System.Formats.Asn1;
using System.Windows.Media;
using System.Xml.Linq;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace Inv_M_Sys.Views.Shared
{
    public partial class ReportsPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Order> SalesList = new();

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
            // Not implemented - depends on Report saving to DB
            MessageBox.Show("Delete not supported in this version.");
        }
        #endregion

        #region Container Buttons
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            string title = ((ComboBoxItem)ReportTypeComboBox.SelectedItem)?.Content?.ToString() ?? "Sales Report";
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content?.ToString() ?? "All";
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;

            var filtered = SalesList.Where(s =>
                (status == "All" || s.Status.ToString() == status) &&
                (!start.HasValue || s.DeliveryDate >= start) &&
                (!end.HasValue || s.DeliveryDate <= end)
            ).ToList();

            SalesListView.ItemsSource = new ObservableCollection<Order>(filtered);
            MessageBox.Show("Report generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenBack_Click(object sender, RoutedEventArgs e)
        {
            GenerateContainer.Visibility = Visibility.Collapsed;
        }

        private void ExpBack_Click(object sender, RoutedEventArgs e)
        {
            ExportReportContainer.Visibility = Visibility.Collapsed;
        }

        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
                csv.WriteRecords(SalesList);

                MessageBox.Show("CSV report exported.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV export failed: " + ex.Message);
            }
        }

        private void ExportAsExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("SalesReport");
                worksheet.Cell(1, 1).Value = "Order ID";
                worksheet.Cell(1, 2).Value = "Customer";
                worksheet.Cell(1, 3).Value = "Delivery Date";
                worksheet.Cell(1, 4).Value = "Total Price";
                worksheet.Cell(1, 5).Value = "Status";

                for (int i = 0; i < SalesList.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = SalesList[i].Id;
                    worksheet.Cell(i + 2, 2).Value = SalesList[i].CustomerName;
                    worksheet.Cell(i + 2, 3).Value = SalesList[i].DeliveryDate;
                    worksheet.Cell(i + 2, 4).Value = SalesList[i].TotalPrice;
                    worksheet.Cell(i + 2, 5).Value = SalesList[i].Status.ToString();
                }

                workbook.SaveAs(filePath);
                MessageBox.Show("Excel report exported.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel export failed: " + ex.Message);
            }
        }

        private void ExportAsPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                PdfDocument doc = new PdfDocument();
                doc.Info.Title = "Sales Report";
                PdfPage page = doc.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Verdana", 12);
                double y = 40;
                gfx.DrawString("Sales Report", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black, new XRect(0, 10, page.Width, 30), XStringFormats.TopCenter);

                foreach (var order in SalesList)
                {
                    string line = $"#{order.Id} - {order.CustomerName} - {order.DeliveryDate:yyyy-MM-dd} - {order.TotalPrice:C} - {order.Status}";
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, 20));
                    y += 20;
                    if (y > page.Height - 40)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }

                doc.Save(filePath);
                MessageBox.Show("PDF report exported.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF export failed: " + ex.Message);
            }
        }
        #endregion

        #region Helpers
        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSalesAsync();
        }

        private async Task LoadSalesAsync()
        {
            SalesList.Clear();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                string query = @"SELECT o.Id, c.FirstName || ' ' || c.LastName AS CustomerName, o.DeliveryDate, o.TotalPrice, o.Status FROM Orders o JOIN Customers c ON o.CustomerId = c.Id WHERE o.IsDeleted = FALSE";

                using var cmd = new NpgsqlCommand(query, conn);
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
                Log.Error(ex, "Failed to load sales report");
                MessageBox.Show("Error loading sales data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            await LoadSalesAsync();
        }
        #endregion
    }
}