using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeTracker.Models;

namespace TimeTracker.ViewModels
{
    internal class MyTasksViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly Employee _currentEmployee;

        private List<Models.Task> _myTasks;
        public List<Models.Task> MyTasks
        {
            get => _myTasks;
            set => SetProperty(ref _myTasks, value);
        }

        private Models.Task _selectedTask;
        public Models.Task SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterTasks();
            }
        }

        private string _selectedStatusFilter = "Все";
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                SetProperty(ref _selectedStatusFilter, value);
                FilterTasks();
            }
        }

        public List<string> StatusFilters { get; } = new List<string>
        {
            "Все", "Новая", "В работе", "На проверке", "Завершена"
        };

        public RelayCommand LoadTasksCommand { get; }
        public RelayCommand StartTaskCommand { get; }
        public RelayCommand CompleteTaskCommand { get; }

        public MyTasksViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            LoadTasksCommand = new RelayCommand(LoadMyTasks);
            StartTaskCommand = new RelayCommand(StartTask, CanStartTask);
            CompleteTaskCommand = new RelayCommand(CompleteTask, CanCompleteTask);

            LoadMyTasks();
        }

        private void LoadMyTasks()
        {
            try
            {
                MyTasks = _context.Tasks
                    .Where(t => t.AssignedTo == _currentEmployee.Id)
                    .OrderByDescending(t => t.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки задач: " + ex.Message);
            }
        }

        private void FilterTasks()
        {
            if (MyTasks == null) return;

            var allTasks = _context.Tasks
                .Where(t => t.AssignedTo == _currentEmployee.Id)
                .ToList();

            var filteredTasks = allTasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedStatusFilter != "Все")
            {
                filteredTasks = filteredTasks.Where(t => t.Status == SelectedStatusFilter);
            }

            MyTasks = filteredTasks.OrderByDescending(t => t.Id).ToList();
        }

        private void StartTask()
        {
            if (SelectedTask != null && SelectedTask.Status == "Новая")
            {
                SelectedTask.Status = "В работе";
                SaveChanges();
                MessageBox.Show($"Задача '{SelectedTask.Title}' начата");
            }
        }

        private bool CanStartTask() => SelectedTask != null && SelectedTask.Status == "Новая";

        private void CompleteTask()
        {
            if (SelectedTask != null &&
                (SelectedTask.Status == "В работе" || SelectedTask.Status == "На проверке"))
            {
                if (MessageBox.Show($"Завершить задачу '{SelectedTask.Title}'?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SelectedTask.Status = "Завершена";
                    SaveChanges();
                    MessageBox.Show($"Задача '{SelectedTask.Title}' завершена");
                }
            }
        }

        private bool CanCompleteTask() => SelectedTask != null &&
            (SelectedTask.Status == "В работе" || SelectedTask.Status == "На проверке");

        private void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
                LoadMyTasks();
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
