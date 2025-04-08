using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.ViewModels;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    public partial class SchedulePage : Page
    {
        private readonly HomeWindow _homeWindow;

        public SchedulePage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            DataContext = new WeeklyScheduleViewModel();
        }

        #region Top Menu Event Handlers
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Home_Click(object sender, RoutedEventArgs e) =>
            _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        #endregion

        #region Staff Selection Event Handlers
        private async void StaffTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StaffTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedType = selectedItem.Content.ToString();
                var vm = DataContext as WeeklyScheduleViewModel;
                var currentRole = vm.CurrentUserRole;

                // Always clear the name combo box first
                vm.StaffNames.Clear();
                StaffNameComboBox.ItemsSource = vm.StaffNames;

                // Restriction for Selling/Stock Staff trying to open other roles
                if ((currentRole == UserRole.SellingStaff && selectedType != "SellingStaff") ||
                    (currentRole == UserRole.StockStaff && selectedType != "StockStaff"))
                {
                    MessageBox.Show("You are only allowed to view schedules of your own role.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StaffTypeComboBox.SelectedIndex = -1;
                    StaffNameComboBox.ItemsSource = null;
                    return;
                }

                // Info message if admin selects admin
                if (currentRole == UserRole.Admin && selectedType == "Admin")
                {
                    MessageBox.Show("You can view admin schedules but not edit them.", "Read Only", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // ✅ For Selling/Stock Staff: only add their own name again every time
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
                    // ✅ Normal logic for Owner and Admin
                    await vm.LoadStaffNamesByType(selectedType);
                }
            }
        }

        private async void StaffNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StaffNameComboBox.SelectedItem is string staffName)
            {
                int staffId = await DatabaseHelper.GetStaffIdByNameAsync(staffName);
                var vm = DataContext as WeeklyScheduleViewModel;

                if (vm != null)
                {
                    // Only allow Selling/Stock Staff to view their own schedule
                    if ((vm.CurrentUserRole == UserRole.SellingStaff || vm.CurrentUserRole == UserRole.StockStaff)
                        && vm.CurrentLoggedStaffId != staffId)
                    {
                        MessageBox.Show("You are only allowed to view your own schedule.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StaffNameComboBox.SelectedIndex = -1;
                        return;
                    }

                    vm.SelectedStaffId = staffId;
                }
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
        #endregion

        #region Submit Changes
        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as WeeklyScheduleViewModel;
            if (vm != null)
            {
                await vm.SaveScheduleAsync();
                MessageBox.Show("Schedule changes saved successfully.");
            }
        }
        #endregion
    }
}