using Inv_M_Sys.Models;
using Inv_M_Sys.Views.Forms;
using Npgsql;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    public partial class SchedulePage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Schedule> ScheduleList { get; set; }
        private Schedule SelectedSchedule { get; set; }

        public SchedulePage(HomeWindow homeWindow)
        {
            InitializeComponent();
            ScheduleList = new ObservableCollection<Schedule>();
            ScheduleListView.ItemsSource = ScheduleList;
            LoadSchedulesAsync();
        }

        private async Task LoadSchedulesAsync()
        {
            ScheduleList.Clear();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    var query = "SELECT Id, EmployeeName, StartTime, EndTime, TotalHours FROM Schedule";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ScheduleList.Add(new Schedule
                            {
                                Id = reader.GetInt32(0),
                                EmployeeName = reader.GetString(1),
                                StartTime = reader.GetTimeSpan(2),
                                EndTime = reader.GetTimeSpan(3),
                                TotalHours = reader.GetDouble(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading schedules.");
                MessageBox.Show("Failed to load schedules.");
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ScheduleFormContainer.Visibility = Visibility.Visible;
            SubmitBtn.Visibility = Visibility.Visible;
            UpdateBtn.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListView.SelectedItem is Schedule schedule)
            {
                SelectedSchedule = schedule;
                EmployeeNameTextBox.Text = schedule.EmployeeName;
                StartTimeTextBox.Text = schedule.StartTime.ToString(@"hh\:mm");
                EndTimeTextBox.Text = schedule.EndTime.ToString(@"hh\:mm");
                TotalHoursTextBox.Text = schedule.TotalHours.ToString();
                ScheduleFormContainer.Visibility = Visibility.Visible;
                SubmitBtn.Visibility = Visibility.Collapsed;
                UpdateBtn.Visibility = Visibility.Visible;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListView.SelectedItem is Schedule schedule)
            {
                var result = MessageBox.Show($"Delete schedule for '{schedule.EmployeeName}'?", "Confirm", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        await conn.OpenAsync();
                        using (var cmd = new NpgsqlCommand("DELETE FROM Schedule WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", schedule.Id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    await LoadSchedulesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error deleting schedule.");
                    MessageBox.Show("Failed to delete schedule.");
                }
            }
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var employeeName = EmployeeNameTextBox.Text;
            var startTime = TimeSpan.Parse(StartTimeTextBox.Text);
            var endTime = TimeSpan.Parse(EndTimeTextBox.Text);
            var totalHours = (endTime - startTime).TotalHours;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    var query = "INSERT INTO Schedule (EmployeeName, StartTime, EndTime, TotalHours) VALUES (@EmployeeName, @StartTime, @EndTime, @TotalHours)";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeName", employeeName);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        cmd.Parameters.AddWithValue("@TotalHours", totalHours);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                ClearForm();
                ScheduleFormContainer.Visibility = Visibility.Collapsed;
                await LoadSchedulesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding schedule.");
                MessageBox.Show("Failed to add schedule.");
            }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSchedule == null || !ValidateForm()) return;

            var employeeName = EmployeeNameTextBox.Text;
            var startTime = TimeSpan.Parse(StartTimeTextBox.Text);
            var endTime = TimeSpan.Parse(EndTimeTextBox.Text);
            var totalHours = (endTime - startTime).TotalHours;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    var query = @"UPDATE Schedule SET 
                                EmployeeName = @EmployeeName, 
                                StartTime = @StartTime, 
                                EndTime = @EndTime, 
                                TotalHours = @TotalHours 
                                WHERE Id = @Id";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeName", employeeName);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        cmd.Parameters.AddWithValue("@TotalHours", totalHours);
                        cmd.Parameters.AddWithValue("@Id", SelectedSchedule.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                ClearForm();
                ScheduleFormContainer.Visibility = Visibility.Collapsed;
                await LoadSchedulesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating schedule.");
                MessageBox.Show("Failed to update schedule.");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ScheduleFormContainer.Visibility = Visibility.Collapsed;
        }

        private void ScheduleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: handle selection if needed
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var query = SearchTextBox.Text.ToLower();
            ScheduleListView.ItemsSource = string.IsNullOrWhiteSpace(query)
                ? ScheduleList
                : ScheduleList.Where(s => s.EmployeeName.ToLower().Contains(query));
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadSchedulesAsync();
        }

        private void ClearForm()
        {
            EmployeeNameTextBox.Text = "";
            StartTimeTextBox.Text = "";
            EndTimeTextBox.Text = "";
            TotalHoursTextBox.Text = "";
            SelectedSchedule = null;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(StartTimeTextBox.Text) ||
                string.IsNullOrWhiteSpace(EndTimeTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }
            return true;
        }
    }
}