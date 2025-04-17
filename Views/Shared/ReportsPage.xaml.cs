using CsvHelper;
using CsvHelper.Configuration;
using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Services.Pages_Services;
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
using Inv_M_Sys.Services.Exports;
using Inv_M_Sys.ViewModels;

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
            var vm = new ReportsPageViewModel();
            DataContext = vm;

            Loaded += async (s, e) => await vm.LoadReportsAsync();
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

        #region Export

        private string GetExportFileName(string extension)
        {
            string title = SelectedReportTitle.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(title))
            {
                return SanitizeFileName(title) + extension;
            }

            string defaultName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}";
            return defaultName + extension;
        }

        private string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input.Replace(" ", "_");
        }

        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = GetExportFileName(".csv")
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ReportExportService.ExportAsCsv(SalesList, dialog.FileName);
                    MessageBox.Show("CSV report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("CSV export failed: " + ex.Message);
                }
            }
        }

        private void ExportAsExcel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = GetExportFileName(".xlsx")
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ReportExportService.ExportAsExcel(SalesList, dialog.FileName);
                    MessageBox.Show("Excel report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Excel export failed: " + ex.Message);
                }
            }
        }

        private void ExportAsPDF_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = GetExportFileName(".pdf")
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string reportTitle = SelectedReportTitle.Text?.Trim() ?? "Sales Report";
                    ReportExportService.ExportAsPdf(SalesList, dialog.FileName, reportTitle);
                    MessageBox.Show("PDF report exported successfully.", "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF export failed: " + ex.Message);
                }
            }
        }

        #endregion
    }
}
