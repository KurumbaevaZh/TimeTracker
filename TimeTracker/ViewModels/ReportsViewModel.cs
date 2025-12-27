
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
                    ReportText = "ОШИБКА: Дата начала не может быть позже даты конца\n\n" +
                                $"Начало: {StartDate:dd.MM.yyyy}\n" +
                                $"Конец: {EndDate:dd.MM.yyyy}";
                    return;
                }

                var timeEntries = _context.TimeEntries
                    .Where(te => te.EmployeeId == _currentEmployee.Id &&
                                te.StartTime.Date >= StartDate.Date &&
                                te.StartTime.Date <= EndDate.Date &&
                                te.EndTime != null &&
                                te.Duration != null)
                    .OrderBy(te => te.StartTime)
                    .ToList();

                Console.WriteLine($"Найдено записей времени: {timeEntries.Count}");
                Console.WriteLine($"Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}");
                Console.WriteLine($"ID сотрудника: {_currentEmployee.Id}");

                if (timeEntries.Any())
                {
                    foreach (var entry in timeEntries)
                    {
                        Console.WriteLine($"Запись: ID={entry.Id}, Start={entry.StartTime}, End={entry.EndTime}, Duration={entry.Duration} мин");
                    }
                }

                int totalMinutes = timeEntries.Sum(te => te.Duration ?? 0);
                string totalTimeFormatted = FormatTime(totalMinutes);
                TotalTimeText = $"Всего времени: {totalTimeFormatted}";
                ReportText = $"ОТЧЕТ ПО РАБОЧЕМУ ВРЕМЕНИ\n\n";
                ReportText += $"Сотрудник: {_currentEmployee.FirstName} {_currentEmployee.LastName}\n";
                ReportText += $"Должность: {_currentEmployee.Position}\n";
                ReportText += $"Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}\n";

                if (timeEntries.Any())
                {
                    ReportText += "ДЕТАЛИ ПО ДНЯМ:\n\n";
                    var byDate = timeEntries.GroupBy(te => te.StartTime.Date)
                                            .OrderBy(g => g.Key);

                    foreach (var dayGroup in byDate)
                    {
                        int dayMinutes = dayGroup.Sum(te => te.Duration ?? 0);
                        string dayTimeFormatted = FormatTime(dayMinutes);

                        ReportText += $"{dayGroup.Key:dd.MM.yyyy} - {dayTimeFormatted}\n";
                        foreach (var entry in dayGroup.OrderBy(e => e.StartTime))
                        {
                            var task = _context.Tasks.FirstOrDefault(t => t.Id == entry.TaskId);
                            string taskName = task?.Title ?? "Рабочий день";

                            string start = entry.StartTime.ToString("HH:mm");
                            string end = entry.EndTime?.ToString("HH:mm") ?? "не завершено";
                            string duration = entry.Duration?.ToString() ?? "0";

                            ReportText += $"  {start}-{end}: {taskName} ({duration} мин)\n";
                        }
                        ReportText += "\n";
                    }
                }
                else
                {
                    ReportText += "За выбранный период не найдено записей рабочего времени.\n";
                }

                ReportText += "\nВЫПОЛНЕННЫЕ ЗАДАЧИ:\n\n";
                var completedTasks = _context.Tasks
                    .Where(t => t.AssignedTo == _currentEmployee.Id &&
                               t.Status == "Завершена")
                    .ToList();

                if (completedTasks.Any())
                {
                    foreach (var task in completedTasks)
                    {
                        ReportText += $"• {task.Title}\n";
                        if (!string.IsNullOrEmpty(task.Description))
                        {
                            ReportText += $"  Описание: {task.Description}\n";
                        }
                        ReportText += $"  Статус: {task.Status}\n\n";
                    }
                }
                else
                {
                    ReportText += "Нет завершенных задач.\n";
                }
            }
            catch (Exception ex)
            {
                ReportText = $"ОШИБКА ПРИ ГЕНЕРАЦИИ ОТЧЕТА:\n{ex.Message}\n\n" +
                            "Пожалуйста, проверьте соединение с базой данных.";
                TotalTimeText = "Ошибка";
                Console.WriteLine($"Ошибка GenerateReport: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private string FormatTime(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0 мин";

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
            {
                return $"{hours} ч {minutes} мин";
            }
            else if (hours > 0)
            {
                return $"{hours} ч";
            }
            else
            {
                return $"{minutes} мин";
            }
        }

        private void ExportToFile()
        {
            try
            {
                var saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Файлы PDF (*.pdf)|*.pdf";
                saveDialog.FileName = $"Отчет_{_currentEmployee.LastName}_{DateTime.Today:yyyyMMdd}";
                saveDialog.DefaultExt = ".txt";

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, ReportText, Encoding.UTF8);
                    TotalTimeText = $"Отчет сохранен: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                TotalTimeText = "Ошибка сохранения файла";
                Console.WriteLine($"Ошибка ExportToFile: {ex.Message}");
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

