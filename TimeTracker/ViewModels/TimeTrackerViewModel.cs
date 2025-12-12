using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using TimeTracker.Models;

namespace TimeTracker.ViewModels
{
    internal class TimeTrackerViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly Employee _currentEmployee;
        private DispatcherTimer _timer;
        private DateTime _workStartTime;
        private TimeSpan _elapsedTimeToday;

        private string _workStatus = "Рабочий день не начат";
        public string WorkStatus
        {
            get => _workStatus;
            set => SetProperty(ref _workStatus, value);
        }

        private string _elapsedTimeText = "00:00:00";
        public string ElapsedTimeText
        {
            get => _elapsedTimeText;
            set => SetProperty(ref _elapsedTimeText, value);
        }

        private bool _isWorkDayStarted;
        public bool IsWorkDayStarted
        {
            get => _isWorkDayStarted;
            set => SetProperty(ref _isWorkDayStarted, value);
        }

        private string _todaySummary = "";
        public string TodaySummary
        {
            get => _todaySummary;
            set => SetProperty(ref _todaySummary, value);
        }

        public RelayCommand StartWorkDayCommand { get; }
        public RelayCommand EndWorkDayCommand { get; }
        public RelayCommand LoadTodaySummaryCommand { get; }

        public TimeTrackerViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            StartWorkDayCommand = new RelayCommand(StartWorkDay, CanStartWorkDay);
            EndWorkDayCommand = new RelayCommand(EndWorkDay, CanEndWorkDay);
            LoadTodaySummaryCommand = new RelayCommand(LoadTodaySummary);

            InitializeTimer();
            LoadTodaySummary();
            CheckIfWorkDayStarted();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (IsWorkDayStarted)
            {
                _elapsedTimeToday = DateTime.Now - _workStartTime;
                ElapsedTimeText = _elapsedTimeToday.ToString(@"hh\:mm\:ss");
            }
        }

        private void CheckIfWorkDayStarted()
        {
            try
            {
                var today = DateTime.Today;
                var todayEntry = _context.TimeEntries
                    .FirstOrDefault(te => te.EmployeeId == _currentEmployee.Id &&
                                         te.StartTime.Date == today &&
                                         te.EndTime == null);

                if (todayEntry != null)
                {
                    _workStartTime = todayEntry.StartTime;
                    IsWorkDayStarted = true;
                    WorkStatus = "Рабочий день начат";
                    _timer.Start();
                    UpdateElapsedTime();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void UpdateElapsedTime()
        {
            if (IsWorkDayStarted)
            {
                _elapsedTimeToday = DateTime.Now - _workStartTime;
                ElapsedTimeText = _elapsedTimeToday.ToString(@"hh\:mm\:ss");
            }
        }

        private void LoadTodaySummary()
        {
            try
            {
                var today = DateTime.Today;
                var todayEntries = _context.TimeEntries
                    .Where(te => te.EmployeeId == _currentEmployee.Id &&
                                te.StartTime.Date == today &&
                                te.EndTime != null)
                    .ToList();

                if (todayEntries.Any())
                {
                    int totalMinutes = todayEntries.Sum(te => te.Duration ?? 0);
                    int hours = totalMinutes / 60;
                    int minutes = totalMinutes % 60;

                    TodaySummary = $"Сегодня отработано: {hours} ч {minutes} мин";
                }
                else
                {
                    TodaySummary = "Сегодня еще не было рабочих сессий";
                }
            }
            catch (Exception ex)
            {
                TodaySummary = "Ошибка загрузки данных";
            }
        }

        private void StartWorkDay()
        {
            try
            {
                _workStartTime = DateTime.Now;
                IsWorkDayStarted = true;
                WorkStatus = "Рабочий день начат";

                var timeEntry = new TimeEntry
                {
                    EmployeeId = _currentEmployee.Id,
                    TaskId = 1, 
                    StartTime = _workStartTime,
                    EndTime = null, 
                    Duration = null
                };

                _context.TimeEntries.Add(timeEntry);
                _context.SaveChanges();

                _timer.Start();
                UpdateElapsedTime();

                MessageBox.Show($"Рабочий день начат в {_workStartTime:HH:mm}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка начала рабочего дня: " + ex.Message);
            }
        }

        private bool CanStartWorkDay() => !IsWorkDayStarted;

        private void EndWorkDay()
        {
            if (!IsWorkDayStarted)
            {
                MessageBox.Show("Рабочий день еще не начат");
                return;
            }

            var endTime = DateTime.Now;
            var workDuration = endTime - _workStartTime;
            int totalMinutes = (int)workDuration.TotalMinutes;
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            try
            {
                var todayEntry = _context.TimeEntries
                    .FirstOrDefault(te => te.EmployeeId == _currentEmployee.Id &&
                                         te.StartTime.Date == DateTime.Today &&
                                         te.EndTime == null);

                if (todayEntry != null)
                {
                    todayEntry.EndTime = endTime;
                    todayEntry.Duration = totalMinutes;
                    _context.SaveChanges();
                }

                _timer.Stop();
                IsWorkDayStarted = false;
                WorkStatus = "Рабочий день завершен";

                LoadTodaySummary();

                MessageBox.Show($"Рабочий день завершен!\n" +
                              $"Начало: {_workStartTime:HH:mm}\n" +
                              $"Конец: {endTime:HH:mm}\n" +
                              $"Отработано: {hours} ч {minutes} мин\n" +
                              $"Всего: {totalMinutes} минут");

                ElapsedTimeText = "00:00:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка завершения рабочего дня: " + ex.Message);
            }
        }

        private bool CanEndWorkDay() => IsWorkDayStarted;

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
