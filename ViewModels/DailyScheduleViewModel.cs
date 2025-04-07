using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Inv_M_Sys.ViewModels
{
    public class DailyScheduleViewModel : INotifyPropertyChanged
    {
        private int _workStartHour, _workStartMinute, _workEndHour, _workEndMinute;
        private int _restStartHour, _restStartMinute, _restEndHour, _restEndMinute;
        private bool _isRestDay;
        private bool _hasRestTime = true;

        public string DayName { get; set; }
        public bool IsRestDay
        {
            get => _isRestDay;
            set { _isRestDay = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); }
        }
        public bool HasRestTime
        {
            get => _hasRestTime;
            set { _hasRestTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); }
        }
        public int WorkStartHour { get => _workStartHour; set { _workStartHour = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int WorkStartMinute { get => _workStartMinute; set { _workStartMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int WorkEndHour { get => _workEndHour; set { _workEndHour = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int WorkEndMinute { get => _workEndMinute; set { _workEndMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int RestStartHour { get => _restStartHour; set { _restStartHour = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int RestStartMinute { get => _restStartMinute; set { _restStartMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int RestEndHour { get => _restEndHour; set { _restEndHour = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }
        public int RestEndMinute { get => _restEndMinute; set { _restEndMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalWorkHours)); } }

        // Total work hours computed based on work duration minus rest duration (if applicable)
        public double TotalWorkHours
        {
            get
            {
                if (IsRestDay) return 0;
                TimeSpan workStart = new TimeSpan(WorkStartHour, WorkStartMinute, 0);
                TimeSpan workEnd = new TimeSpan(WorkEndHour, WorkEndMinute, 0);
                TimeSpan workDuration = workEnd - workStart;
                if (HasRestTime)
                {
                    TimeSpan restStart = new TimeSpan(RestStartHour, RestStartMinute, 0);
                    TimeSpan restEnd = new TimeSpan(RestEndHour, RestEndMinute, 0);
                    TimeSpan restDuration = restEnd - restStart;
                    return (workDuration - restDuration).TotalHours;
                }
                else
                {
                    return workDuration.TotalHours;
                }
            }
        }

        public ObservableCollection<int> Hours { get; } = new ObservableCollection<int>(Enumerable.Range(0, 24));
        public ObservableCollection<int> Minutes { get; } = new ObservableCollection<int>(new[] { 0, 15, 30, 45 });

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
