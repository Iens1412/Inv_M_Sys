using Inv_M_Sys.Models;
using Inv_M_Sys.Services.Pages_Services;
using Inv_M_Sys.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Windows;
using System.Threading.Tasks;
using Serilog;

namespace Inv_M_Sys.ViewModels
{
    public class ReportsPageViewModel : BaseViewModel
    {
        // Collections bound to the UI
        public ObservableCollection<Report> Reports { get; } = new();
        public ObservableCollection<Order> SelectedReportOrders { get; } = new();

        // Currently selected report
        private Report _selectedReport;
        public Report SelectedReport
        {
            get => _selectedReport;
            set
            {
                if (SetProperty(ref _selectedReport, value))
                {
                    LoadSelectedReportDetailsAsync();
                    OnPropertyChanged(nameof(CanExport));
                }
            }
        }

        // Text displayed above the selected report details
        private string _selectedReportTitle;
        public string SelectedReportTitle
        {
            get => _selectedReportTitle;
            set => SetProperty(ref _selectedReportTitle, value);
        }

        // Description or date range of selected report
        private string _selectedReportDetails;
        public string SelectedReportDetails
        {
            get => _selectedReportDetails;
            set => SetProperty(ref _selectedReportDetails, value);
        }

        // Input from user for new report title
        private string _reportTitleInput;
        public string ReportTitleInput
        {
            get => _reportTitleInput;
            set => SetProperty(ref _reportTitleInput, value);
        }

        // Text used for searching
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        // Tracks if Generate or Export container should be visible
        public bool IsAnyContainerVisible => IsGenerateVisible || IsExportVisible;
        private void UpdateContainerVisibility() => OnPropertyChanged(nameof(IsAnyContainerVisible));

        // Toggles Generate container visibility
        private bool _isGenerateVisible;
        public bool IsGenerateVisible
        {
            get => _isGenerateVisible;
            set
            {
                if (SetProperty(ref _isGenerateVisible, value))
                    UpdateContainerVisibility();
            }
        }

        // Toggles Export container visibility
        private bool _isExportVisible;
        public bool IsExportVisible
        {
            get => _isExportVisible;
            set
            {
                if (SetProperty(ref _isExportVisible, value))
                    UpdateContainerVisibility();
            }
        }

        // Search filtering category (Reports or Details)
        private string _searchCategory = "Reports";
        public string SearchCategory
        {
            get => _searchCategory;
            set => SetProperty(ref _searchCategory, value);
        }

        // Filters for generating reports
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedStatus { get; set; } = "All";

        // Holds unfiltered data for search operations
        private List<Report> _allReports = new();

        // Progress indicator
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        private bool _isLoading;

        // Options for dropdowns
        public ObservableCollection<string> SearchCategories { get; } = new() { "Reports", "Details" };
        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Delivered", "Cancelled" };

        // Commands
        public ICommand RefreshReportsCommand { get; }
        public ICommand DeleteReportCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ExpBackCommand { get; }
        public ICommand CancelGenerateCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ICommand ShowNewCommand => new RelayCommand(() => IsGenerateVisible = true);
        public ICommand ShowExportCommand => new RelayCommand(() => IsExportVisible = true);

        // Enable export buttons only when a report is selected
        public bool CanExport => SelectedReport != null;

        // Constructor
        public ReportsPageViewModel()
        {
            RefreshReportsCommand = new RelayCommand(async () => await LoadReportsAsync());
            DeleteReportCommand = new RelayCommand(async () => await DeleteSelectedReportAsync());
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            ExpBackCommand = new RelayCommand(ExecuteExpBack);
            CancelGenerateCommand = new RelayCommand(ExecuteGenerateBack);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearSearchCommand = new RelayCommand(async () => await LoadReportsAsync());
        }

        // Loads all reports from DB into list
        public async Task LoadReportsAsync()
        {
            try
            {
                IsLoading = true;
                Reports.Clear();

                _allReports = await ReportsService.GetAllReportsAsync();
                foreach (var report in _allReports)
                    Reports.Add(report);

                Log.Information("Loaded {Count} reports.", Reports.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load reports.");
                MessageBox.Show("Failed to load reports: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Loads the orders/details for the selected report
        private async void LoadSelectedReportDetailsAsync()
        {
            if (SelectedReport == null)
            {
                SelectedReportOrders.Clear();
                SelectedReportTitle = "No Report Selected";
                SelectedReportDetails = "";
                return;
            }

            try
            {
                SelectedReportOrders.Clear();
                var orders = await ReportsService.GetReportDetailsAsync(SelectedReport.Id);
                foreach (var order in orders)
                    SelectedReportOrders.Add(order);

                SelectedReportTitle = SelectedReport.ReportTitle;
                SelectedReportDetails = $"From {SelectedReport.StartDate:yyyy-MM-dd} to {SelectedReport.EndDate:yyyy-MM-dd} ({SelectedReport.Status})";

                Log.Information("Loaded {Count} orders for report #{Id}.", orders.Count, SelectedReport.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load details for report #{Id}.", SelectedReport.Id);
                MessageBox.Show("Failed to load report details: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Deletes a selected report
        private async Task DeleteSelectedReportAsync()
        {
            if (SelectedReport == null)
            {
                MessageBox.Show("Please select a report to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this report?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await ReportsService.DeleteReportAsync(SelectedReport.Id);
                await LoadReportsAsync();
                SelectedReport = null;

                Log.Information("Report #{Id} deleted.", SelectedReport?.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete report #{Id}.", SelectedReport?.Id);
                MessageBox.Show("Failed to delete report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Generates a new report
        private async Task GenerateReportAsync()
        {
            if (string.IsNullOrWhiteSpace(ReportTitleInput))
            {
                MessageBox.Show("Please enter a title for the report.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var reportId = await ReportGenerationService.GenerateReportAsync(ReportTitleInput, SelectedStatus, StartDate, EndDate);

                await Task.Delay(2000); // Simulate generation time

                MessageBox.Show("Report generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadReportsAsync();

                Log.Information("Generated report #{Id} with title '{Title}'", reportId, ReportTitleInput);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate report.");
                MessageBox.Show("Failed to generate report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Collapses the export container
        private void ExecuteExpBack() => IsExportVisible = false;

        // Collapses the generate container
        private void ExecuteGenerateBack() => IsGenerateVisible = false;

        // Filters the report list or order list based on search category
        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            if (SearchCategory == "Reports")
            {
                var filtered = _allReports
                    .Where(o =>
                        o.Id.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.ReportTitle.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.ReportType.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Reports.Clear();
                foreach (var report in filtered)
                    Reports.Add(report);

                Log.Information("Filtered reports with '{Search}' - {Count} found.", SearchText, Reports.Count);
            }
            else if (SearchCategory == "Details")
            {
                var filteredOrders = SelectedReportOrders
                    .Where(o =>
                        o.Id.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.CustomerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.Status.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                SelectedReportOrders.Clear();
                foreach (var order in filteredOrders)
                    SelectedReportOrders.Add(order);

                Log.Information("Filtered report details with '{Search}' - {Count} found.", SearchText, SelectedReportOrders.Count);
            }
        }
    }
}
