using Inv_M_Sys.Models;
using Inv_M_Sys.Services.Pages_Services;
using Inv_M_Sys.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Windows;
using System.Threading.Tasks;

namespace Inv_M_Sys.ViewModels
{
    public class ReportsPageViewModel : BaseViewModel
    {
        public ObservableCollection<Report> Reports { get; } = new();
        public ObservableCollection<Order> SelectedReportOrders { get; } = new();

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

        private string _selectedReportTitle;
        public string SelectedReportTitle
        {
            get => _selectedReportTitle;
            set => SetProperty(ref _selectedReportTitle, value);
        }

        private string _selectedReportDetails;
        public string SelectedReportDetails
        {
            get => _selectedReportDetails;
            set => SetProperty(ref _selectedReportDetails, value);
        }

        private string _reportTitleInput;
        public string ReportTitleInput
        {
            get => _reportTitleInput;
            set => SetProperty(ref _reportTitleInput, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
        
        public bool IsAnyContainerVisible => IsGenerateVisible || IsExportVisible;

        private void UpdateContainerVisibility()
        {
            OnPropertyChanged(nameof(IsAnyContainerVisible));
        }

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

        private string _searchCategory = "Reports";
        public string SearchCategory
        {
            get => _searchCategory;
            set => SetProperty(ref _searchCategory, value);
        }


        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedStatus { get; set; } = "All";
        private List<Report> _allReports = new();

        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        private bool _isLoading;

        public ObservableCollection<string> SearchCategories { get; } = new() { "Reports", "Details" };
        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            "All", "Pending", "Delivered", "Cancelled"
        };


        public ICommand RefreshReportsCommand { get; }
        public ICommand DeleteReportCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ExpBackCommand { get; }
        public ICommand CancelGenerateCommand { get; }

        public ICommand ShowNewCommand => new RelayCommand(() => IsGenerateVisible = true);
        public ICommand ShowExportCommand => new RelayCommand(() => IsExportVisible = true);

        public bool CanExport => SelectedReport != null;

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

        public async Task LoadReportsAsync()
        {
            IsLoading = true;
            Reports.Clear();

            _allReports = await ReportsService.GetAllReportsAsync();
            foreach (var report in _allReports)
                Reports.Add(report);

            IsLoading = false;
        }

        private async void LoadSelectedReportDetailsAsync()
        {
            if (SelectedReport == null)
            {
                SelectedReportOrders.Clear();
                SelectedReportTitle = "No Report Selected";
                SelectedReportDetails = "";
                return;
            }

            SelectedReportOrders.Clear();
            var orders = await ReportsService.GetReportDetailsAsync(SelectedReport.Id);
            foreach (var order in orders)
                SelectedReportOrders.Add(order);

            SelectedReportTitle = SelectedReport.ReportTitle;
            SelectedReportDetails = $"From {SelectedReport.StartDate:yyyy-MM-dd} to {SelectedReport.EndDate:yyyy-MM-dd} ({SelectedReport.Status})";
        }

        private async Task DeleteSelectedReportAsync()
        {
            if (SelectedReport == null)
            {
                MessageBox.Show("Please select a report to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this report?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            await ReportsService.DeleteReportAsync(SelectedReport.Id);
            await LoadReportsAsync();
            SelectedReport = null;
        }

        //Gernrate a report
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

                await Task.Delay(2000);

                MessageBox.Show("Report generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadReportsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        //Commands for the back buttons in the containers
        private void ExecuteExpBack() => IsExportVisible = false;
        private void ExecuteGenerateBack() => IsGenerateVisible = false;

        //Search functions
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
            }
        }

    }
}