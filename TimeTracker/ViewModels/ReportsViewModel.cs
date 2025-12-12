
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using TimeTracker.Models;

namespace TimeTracker.ViewModels
{
    internal class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly Employee _currentEmployee;

        private DateTime _startDate = DateTime.Today.AddDays(-7);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _reportText = "";
        public string ReportText
        {
            get => _reportText;
            set => SetProperty(ref _reportText, value);
        }

        private int _totalMinutes;
        public int TotalMinutes
        {
            get => _totalMinutes;
            set => SetProperty(ref _totalMinutes, value);
        }

        private string _totalTimeText = "";
        public string TotalTimeText
        {
            get => _totalTimeText;
            set => SetProperty(ref _totalTimeText, value);
        }

        public RelayCommand GenerateReportCommand { get; }
        public RelayCommand ExportToFileCommand { get; }

        public ReportsViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportToFileCommand = new RelayCommand(ExportToFile);

            GenerateReport();
        }

        private void GenerateReport()
        {
            try
            {
                if (StartDate > EndDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты конца");
                    return;
                }

                var timeEntries = _context.TimeEntries
                    .Where(te => te.EmployeeId == _currentEmployee.Id &&
                                te.StartTime.Date >= StartDate &&
                                te.StartTime.Date <= EndDate &&
                                te.EndTime != null)
                    .ToList();

                TotalMinutes = timeEntries.Sum(te => te.Duration ?? 0);

                int hours = TotalMinutes / 60;
                int minutes = TotalMinutes % 60;
                TotalTimeText = $"Всего времени: {hours} ч {minutes} мин";

                ReportText = $"ОТЧЕТ ПО РАБОЧЕМУ ВРЕМЕНИ \n\n";
                ReportText += $"Сотрудник: {_currentEmployee.FirstName} {_currentEmployee.LastName}\n";
                ReportText += $"Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}\n";
                ReportText += $"Общее время: {hours} ч {minutes} мин ({TotalMinutes} минут)\n\n";

                ReportText += "ДЕТАЛИ ПО ДНЯМ \n\n";

                var byDate = timeEntries.GroupBy(te => te.StartTime.Date);

                foreach (var dayGroup in byDate.OrderBy(g => g.Key))
                {
                    int dayMinutes = dayGroup.Sum(te => te.Duration ?? 0);
                    int dayHours = dayMinutes / 60;
                    int dayMins = dayMinutes % 60;

                    ReportText += $"{dayGroup.Key:dd.MM.yyyy} - {dayHours} ч {dayMins} мин\n";

                    foreach (var entry in dayGroup.OrderBy(e => e.StartTime))
                    {
                        var task = _context.Tasks.FirstOrDefault(t => t.Id == entry.TaskId);
                        string taskName = task?.Title ?? "Рабочий день";

                        ReportText += $"  {entry.StartTime:HH:mm}-{entry.EndTime:HH:mm}: {taskName} ({entry.Duration} мин)\n";
                    }
                    ReportText += "\n";
                }

                ReportText += "ВЫПОЛНЕННЫЕ ЗАДАЧИ \n\n";

                var completedTasks = _context.Tasks
                    .Where(t => t.AssignedTo == _currentEmployee.Id && t.Status == "Завершена")
                    .ToList();

                if (completedTasks.Any())
                {
                    foreach (var task in completedTasks)
                    {
                        ReportText += $"• {task.Title}\n";
                        if (!string.IsNullOrEmpty(task.Description))
                        {
                            ReportText += $"  {task.Description}\n";
                        }
                        ReportText += "\n";
                    }
                }
                else
                {
                    ReportText += "Нет завершенных задач\n";
                }

                MessageBox.Show("Отчет сгенерирован!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void ExportToFile()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog();
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
                saveDialog.FileName = $"Отчет_{_currentEmployee.LastName}_{DateTime.Today:yyyyMMdd}.txt";

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, ReportText);
                    MessageBox.Show($"Отчет сохранен в файл:\n{saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

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

