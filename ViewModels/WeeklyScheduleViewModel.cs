using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Npgsql;
using Serilog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Inv_M_Sys.ViewModels
{
    public class WeeklyScheduleViewModel : BaseViewModel
    {
        public ObservableCollection<DailyScheduleViewModel> DailySchedules { get; set; } = new();

        private ObservableCollection<string> _staffNames = new();
        public ObservableCollection<string> StaffNames
        {
            get => _staffNames;
            set => SetProperty(ref _staffNames, value);
        }

        public UserRole CurrentUserRole { get; set; } = GetUserRoleFromSession();
        public int CurrentLoggedStaffId { get; set; } = GetUserIdFromSession();

        private UserRole _selectedStaffRole;
        public UserRole SelectedStaffRole
        {
            get => _selectedStaffRole;
            set => SetProperty(ref _selectedStaffRole, value);
        }

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

        /// <summary>
        /// Indicates if the currently logged-in user is allowed to view the selected staff's schedule.
        /// </summary>
        public bool CanViewSchedule
        {
            get
            {
                if (CurrentUserRole == UserRole.Owner)
                    return true;

                if (CurrentUserRole == UserRole.Admin)
                    return true; // Admins can view all

                if (CurrentUserRole == UserRole.SellingStaff && SelectedStaffRole == UserRole.SellingStaff)
                    return SelectedStaffId == CurrentLoggedStaffId;

                if (CurrentUserRole == UserRole.StockStaff || SelectedStaffRole == UserRole.StockStaff)
                    return SelectedStaffId == CurrentLoggedStaffId;

                return false;
            }
        }

        public bool IsScheduleEditable
        {
            get
            {
                if (CurrentUserRole == UserRole.Owner)
                    return true;

                if (CurrentUserRole == UserRole.Admin)
                {
                    return (SelectedStaffRole == UserRole.SellingStaff || SelectedStaffRole == UserRole.StockStaff);
                }

                return false; // Selling and Stock Staff can't edit anything
            }
        }

        public double TotalWeeklyHours => DailySchedules.Sum(d => d.TotalWorkHours);

        private string _saveStatus;
        public string SaveStatus
        {
            get => _saveStatus;
            set => SetProperty(ref _saveStatus, value);
        }

        public WeeklyScheduleViewModel()
        {
            DailySchedules = new ObservableCollection<DailyScheduleViewModel>();
        }

        /// <summary>
        /// Loads weekly schedule for selected staff member.
        /// If not found, loads default schedule.
        /// </summary>
        public async Task LoadStaffNamesByType(string staffType)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                string query = "SELECT FirstName || ' ' || LastName AS FullName FROM Users WHERE Role = @StaffType";
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StaffType", staffType);
                using var reader = await cmd.ExecuteReaderAsync();
                StaffNames.Clear();
                while (await reader.ReadAsync())
                {
                    StaffNames.Add(reader.GetString(0));
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Error loading staff names in view model.");
            }
        }

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
                foreach (var day in GetDefaultSchedule())
                {
                    DailySchedules.Add(day);
                    day.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalWeeklyHours));
                }
            }
            OnPropertyChanged(nameof(TotalWeeklyHours));
        }

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
        /// Saves the current weekly schedule for the selected staff.
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
            catch (System.Exception ex)
            {
                SaveStatus = "Error saving changes";
                Log.Error(ex, "Error saving weekly schedule");
            }
        }

        private static UserRole GetUserRoleFromSession()
        {
            return Enum.TryParse(SessionManager.CurrentUserRole?.Replace(" ", ""), out UserRole role)
                ? role
                : UserRole.SellingStaff;
        }

        private static int GetUserIdFromSession()
        {
            return SessionManager.CurrentUserId ?? SessionManager.CurrentOwnerId ?? 0;
        }
    }
}

