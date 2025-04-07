using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Serilog;

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

        // Current user's role (e.g., "Owner", "Admin", etc.)
        public string CurrentUserRole { get; set; } = "Owner"; // Adjust based on logged-in user

        // Selected staff Id; when set, triggers loading of the schedule.
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
                        // Clear the schedule if no valid staff is selected
                        DailySchedules.Clear();
                        OnPropertyChanged(nameof(TotalWeeklyHours));
                    }
                    else
                    {
                        LoadStaffScheduleAsync();
                    }
                }
            }
        }

        // Total weekly work hours computed from daily schedules.
        public double TotalWeeklyHours => DailySchedules.Sum(d => d.TotalWorkHours);

        // Status message to show save confirmation.
        private string _saveStatus;
        public string SaveStatus
        {
            get => _saveStatus;
            set => SetProperty(ref _saveStatus, value);
        }

        // Constructor: initialize an empty schedule.
        public WeeklyScheduleViewModel()
        {
            DailySchedules = new ObservableCollection<DailyScheduleViewModel>();
        }

        /// <summary>
        /// Loads staff names from the Users table based on the selected staff type.
        /// </summary>
        /// <param name="staffType">The staff type (e.g., "SellingStaff", "StockStaff", or "Admin").</param>
        public async Task LoadStaffNamesByType(string staffType)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
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
            catch (System.Exception ex)
            {
                Log.Error(ex, "Error loading staff names in view model.");
            }
        }

        /// <summary>
        /// Loads the weekly schedule for the selected staff.
        /// If no schedule exists for this staff, load the default schedule.
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
                // No schedule exists for this staff—load default schedule.
                var defaultSchedule = GetDefaultSchedule();
                foreach (var day in defaultSchedule)
                {
                    DailySchedules.Add(day);
                    day.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalWeeklyHours));
                }
            }
            OnPropertyChanged(nameof(TotalWeeklyHours));
        }

        /// <summary>
        /// Returns a new default weekly schedule.
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
        /// Saves the current weekly schedule for the selected staff to the database.
        /// </summary>
        public async Task SaveScheduleAsync()
        {
            try
            {
                foreach (var day in DailySchedules)
                {
                    await DatabaseHelper.SaveDailyScheduleAsync(day, SelectedStaffId);
                }
                SaveStatus = "Changes saved";
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
    }
}
