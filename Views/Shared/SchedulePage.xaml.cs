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
                string staffType = selectedItem.Content.ToString();
                // Only owners can plan for Admin staff.
                var currentRole = ((WeeklyScheduleViewModel)DataContext).CurrentUserRole;
                if (staffType == "Admin" && currentRole != "Owner")
                {
                    MessageBox.Show("Only owners can plan for Admin staff.");
                    StaffTypeComboBox.SelectedIndex = -1;
                    return;
                }
                await ((WeeklyScheduleViewModel)DataContext).LoadStaffNamesByType(staffType);
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
                    vm.SelectedStaffId = staffId;
                }
            }
            else
            {
                // Clear schedule if no staff is selected
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

