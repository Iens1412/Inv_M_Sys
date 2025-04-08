using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Npgsql;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Inv_M_Sys.ViewModels
{
    public class WeeklyScheduleViewModel : BaseViewModel
    {
        // Collection of daily schedules for the week.
        public ObservableCollection<DailyScheduleViewModel> DailySchedules { get; set; }

        // Collection of staff names loaded from the database.
        private ObservableCollection<string> _staffNames = new ObservableCollection<string>();
        public ObservableCollection<string> StaffNames
        {
            get => _staffNames;
            set => SetProperty(ref _staffNames, value);
        }

        // The role of the current logged-in user.
        public UserRole CurrentUserRole { get; set; }

        // The currently logged staff's ID.
        public int CurrentLoggedStaffId { get; set; }

        // The role of the selected staff (loaded from the database).
        private UserRole _selectedStaffRole;
        public UserRole SelectedStaffRole
        {
            get => _selectedStaffRole;
            set => SetProperty(ref _selectedStaffRole, value);
        }

        // The ID of the staff whose schedule is being viewed/edited.
        private int _selectedStaffId;
        public int SelectedStaffId
        {
            get => _selectedStaffId;
            set
            {
                if (SetProperty(ref _selectedStaffId, value))
                {
                    if (_selectedStaffId == 0)
                    {
                        DailySchedules.Clear();
                        OnPropertyChanged(nameof(TotalWeeklyHours));
                    }
                    else
                    {
                        LoadStaffScheduleAsync();
                        // Update the selected staff's role.
                        Task.Run(async () =>
                        {
                            SelectedStaffRole = await DatabaseHelper.GetStaffRoleByIdAsync(_selectedStaffId);
                            OnPropertyChanged(nameof(CanViewSchedule));
                            OnPropertyChanged(nameof(IsScheduleEditable));
                        });
                    }
                }
            }
        }

        // Determines whether the selected schedule can be viewed.
        public bool CanViewSchedule
        {
            get
            {
                // Owners can view everything.
                if (CurrentUserRole == UserRole.Owner)
                    return true;
                // Admins can view any schedule if the schedule belongs to an admin;
                // for other roles, they may view only their own schedule.
                if (CurrentUserRole == UserRole.Admin)
                {
                    if (SelectedStaffRole == UserRole.Admin)
                        return true;
                    return SelectedStaffId == CurrentLoggedStaffId;
                }
                // Selling or Stock staff can view only their own schedule.
                return SelectedStaffId == CurrentLoggedStaffId;
            }
        }

        // Determines whether the schedule can be edited.
        public bool IsScheduleEditable
        {
            get
            {
                // Owners can edit everything.
                if (CurrentUserRole == UserRole.Owner)
                    return true;
                // Admins can edit only their own schedule.
                if (CurrentUserRole == UserRole.Admin)
                    return SelectedStaffId == CurrentLoggedStaffId;
                // Selling and Stock staff cannot edit schedules.
                return false;
            }
        }

        // Total weekly work hours computed from daily schedules.
        public double TotalWeeklyHours => DailySchedules.Sum(d => d.TotalWorkHours);

        // Save status message to display in the UI.
        private string _saveStatus;
        public string SaveStatus
        {
            get => _saveStatus;
            set => SetProperty(ref _saveStatus, value);
        }

        public WeeklyScheduleViewModel()
        {
            DailySchedules = new ObservableCollection<DailyScheduleViewModel>();

            // Set the current user's role and ID from SessionManager.
            if (!string.IsNullOrEmpty(SessionManager.CurrentUserRole) &&
                Enum.TryParse(SessionManager.CurrentUserRole, true, out UserRole loggedRole))
            {
                CurrentUserRole = loggedRole;
            }
            else
            {
                // Fallback default role if not set
                CurrentUserRole = UserRole.SellingStaff;
            }

            CurrentLoggedStaffId = SessionManager.CurrentUserId.HasValue ? SessionManager.CurrentUserId.Value : 0;
        }

        /// <summary>
        /// Loads staff names from the database based on the given staff type.
        /// </summary>
        /// <param name="staffType">The staff type (for example "SellingStaff" or "StockStaff").</param>
        public async Task LoadStaffNamesByType(string staffType)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    // Query concatenates first and last name to form the full name.
                    string query = "SELECT FirstName || ' ' || LastName AS FullName FROM Users WHERE Role = @StaffType";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StaffType", staffType);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            StaffNames.Clear();
                            while (await reader.ReadAsync())
                            {
                                StaffNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading staff names in view model.");
            }
        }

        /// <summary>
        /// Loads the weekly schedule for the selected staff from the database.
        /// If no schedule exists for the staff, load a default schedule.
        /// </summary>
        private async void LoadStaffScheduleAsync()
        {
            var loadedSchedule = await DatabaseHelper.LoadWeeklyScheduleForStaffAsync(SelectedStaffId);
            DailySchedules.Clear();
            if (loadedSchedule != null && loadedSchedule.DailySchedules.Any())
            {
                foreach (var day in loadedSchedule.DailySchedules)
                {
                    DailySchedules.Add(day);
                    day.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalWeeklyHours));
                }
            }
            else
            {
                // If no schedule exists, load a default schedule.
                foreach (var day in GetDefaultSchedule())
                {
                    DailySchedules.Add(day);
                    day.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalWeeklyHours));
                }
            }
            OnPropertyChanged(nameof(TotalWeeklyHours));
        }

        /// <summary>
        /// Provides a default weekly schedule.
        /// </summary>
        private ObservableCollection<DailyScheduleViewModel> GetDefaultSchedule()
        {
            return new ObservableCollection<DailyScheduleViewModel>
            {
                new DailyScheduleViewModel { DayName = "Monday", WorkStartHour = 8, WorkStartMinute = 0, WorkEndHour = 17, WorkEndMinute = 0, RestStartHour = 12, RestStartMinute = 0, RestEndHour = 13, RestEndMinute = 0, HasRestTime = true, IsRestDay = false },
                new DailyScheduleViewModel { DayName = "Tuesday", WorkStartHour = 8, WorkStartMinute = 0, WorkEndHour = 17, WorkEndMinute = 0, RestStartHour = 12, RestStartMinute = 0, RestEndHour = 13, RestEndMinute = 0, HasRestTime = true, IsRestDay = false },
                new DailyScheduleViewModel { DayName = "Wednesday", WorkStartHour = 8, WorkStartMinute = 0, WorkEndHour = 17, WorkEndMinute = 0, RestStartHour = 12, RestStartMinute = 0, RestEndHour = 13, RestEndMinute = 0, HasRestTime = true, IsRestDay = false },
                new DailyScheduleViewModel { DayName = "Thursday", WorkStartHour = 8, WorkStartMinute = 0, WorkEndHour = 17, WorkEndMinute = 0, RestStartHour = 12, RestStartMinute = 0, RestEndHour = 13, RestEndMinute = 0, HasRestTime = true, IsRestDay = false },
                new DailyScheduleViewModel { DayName = "Friday", WorkStartHour = 8, WorkStartMinute = 0, WorkEndHour = 17, WorkEndMinute = 0, RestStartHour = 12, RestStartMinute = 0, RestEndHour = 13, RestEndMinute = 0, HasRestTime = true, IsRestDay = false },
                new DailyScheduleViewModel { DayName = "Saturday", IsRestDay = true, HasRestTime = false },
                new DailyScheduleViewModel { DayName = "Sunday", IsRestDay = true, HasRestTime = false }
            };
        }

        /// <summary>
        /// Saves the current weekly schedule to the database for the selected staff.
        /// Displays a temporary status message upon successful save.
        /// </summary>
        public async Task SaveScheduleAsync()
        {
            try
            {
                foreach (var day in DailySchedules)
                {
                    await DatabaseHelper.SaveDailyScheduleAsync(day, SelectedStaffId);
                }
                SaveStatus = "Saving Changes";
                await Task.Delay(2000);
                SaveStatus = string.Empty;
                OnPropertyChanged(nameof(TotalWeeklyHours));
            }
            catch (Exception ex)
            {
                SaveStatus = "Error saving changes";
                Log.Error(ex, "Error saving weekly schedule");
            }
        }
    }
}
