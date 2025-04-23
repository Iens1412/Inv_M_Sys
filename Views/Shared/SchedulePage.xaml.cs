using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.ViewModels;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for SchedulePage.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        private readonly HomeWindow _homeWindow;

        /// <summary>
        /// Initializes the SchedulePage and binds the WeeklyScheduleViewModel.
        /// </summary>
        public SchedulePage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            DataContext = new WeeklyScheduleViewModel();
        }

        #region Top Menu Event Handlers

        /// <summary>
        /// Logs out the current user and ends the session.
        /// </summary>
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        /// <summary>
        /// Closes the application and expires the current session.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Minimizes the current window.
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            Window.GetWindow(this).WindowState = WindowState.Minimized;

        /// <summary>
        /// Navigates back to the home dashboard.
        /// </summary>
        private void Home_Click(object sender, RoutedEventArgs e) =>
            _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        #endregion

        #region Staff Selection Event Handlers

        /// <summary>
        /// Loads staff names based on selected role and applies access restrictions.
        /// </summary>
        private async void StaffTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (StaffTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedType = selectedItem.Content.ToString();
                    var vm = DataContext as WeeklyScheduleViewModel;
                    var currentRole = vm.CurrentUserRole;

                    vm.StaffNames.Clear();
                    StaffNameComboBox.ItemsSource = vm.StaffNames;

                    if ((currentRole == UserRole.SellingStaff && selectedType != "SellingStaff") ||
                        (currentRole == UserRole.StockStaff && selectedType != "StockStaff"))
                    {
                        MessageBox.Show("You are only allowed to view schedules of your own role.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StaffTypeComboBox.SelectedIndex = -1;
                        StaffNameComboBox.ItemsSource = null;
                        return;
                    }

                    if (currentRole == UserRole.Admin && selectedType == "Admin")
                    {
                        MessageBox.Show("You can view admin schedules but not edit them.", "Read Only", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (currentRole == UserRole.SellingStaff || currentRole == UserRole.StockStaff)
                    {
                        string fullName = await DatabaseHelper.GetStaffFullNameByIdAsync(vm.CurrentLoggedStaffId);
                        if (!string.IsNullOrWhiteSpace(fullName))
                        {
                            vm.StaffNames.Add(fullName);
                            StaffNameComboBox.SelectedItem = fullName;
                        }
                    }
                    else
                    {
                        await vm.LoadStaffNamesByType(selectedType);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during staff type selection.");
                MessageBox.Show($"Error loading staff types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the schedule for the selected staff member, applying role-based restrictions.
        /// </summary>
        private async void StaffNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (StaffNameComboBox.SelectedItem is string staffName)
                {
                    int staffId = await DatabaseHelper.GetStaffIdByNameAsync(staffName);
                    var vm = DataContext as WeeklyScheduleViewModel;

                    if ((vm.CurrentUserRole == UserRole.SellingStaff || vm.CurrentUserRole == UserRole.StockStaff)
                        && vm.CurrentLoggedStaffId != staffId)
                    {
                        MessageBox.Show("You are only allowed to view your own schedule.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StaffNameComboBox.SelectedIndex = -1;
                        return;
                    }

                    vm.SelectedStaffId = staffId;
                }
                else
                {
                    var vm = DataContext as WeeklyScheduleViewModel;
                    if (vm != null)
                    {
                        vm.SelectedStaffId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during staff name selection.");
                MessageBox.Show($"Error selecting staff name: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Submit Changes

        /// <summary>
        /// Saves the current weekly schedule to the database.
        /// </summary>
        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as WeeklyScheduleViewModel;
                if (vm != null)
                {
                    await vm.SaveScheduleAsync();
                    MessageBox.Show("Schedule changes saved successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving weekly schedule.");
                MessageBox.Show($"Error saving schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
