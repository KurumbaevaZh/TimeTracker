using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.ViewModels
{
    internal class TaskManagerViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private Employee _currentEmployee;

        private List<Models.Task> _tasks;
        public List<Models.Task> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        private List<Project> _projects;
        public List<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private List<Employee> _employees;
        public List<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
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

        private Project _selectedProjectFilter;
        public Project SelectedProjectFilter
        {
            get => _selectedProjectFilter;
            set
            {
                SetProperty(ref _selectedProjectFilter, value);
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
        public RelayCommand CreateTaskCommand { get; }
        public RelayCommand EditTaskCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand ChangeStatusCommand { get; }

        public TaskManagerViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            LoadTasksCommand = new RelayCommand(LoadData);
            CreateTaskCommand = new RelayCommand(CreateTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            ChangeStatusCommand = new RelayCommand(ChangeStatus, CanChangeStatus);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Tasks = _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.Assignee)
                    .ToList();

                Projects = _context.Projects.ToList();
                Employees = _context.Employees.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void FilterTasks()
        {
            if (Tasks == null) return;

            var filteredTasks = Tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedProjectFilter != null)
            {
                filteredTasks = filteredTasks.Where(t => t.ProjectId == SelectedProjectFilter.Id);
            }

            if (SelectedStatusFilter != "Все")
            {
                filteredTasks = filteredTasks.Where(t => t.Status == SelectedStatusFilter);
            }

            Tasks = filteredTasks.ToList();
        }

        private void CreateTask()
        {
            var newTask = new Models.Task
            {
                Title = "Новая задача",
                Description = "Описание задачи",
                Status = "Новая"
            };

            Tasks.Add(newTask);
            SelectedTask = newTask;
        }

        private void EditTask()
        {
            if (SelectedTask != null)
            {
                MessageBox.Show($"Редактирование задачи: {SelectedTask.Title}");
            }
        }

        private bool CanEditTask() => SelectedTask != null;

        private void DeleteTask()
        {
            if (SelectedTask != null && MessageBox.Show("Удалить задачу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        private bool CanDeleteTask() => SelectedTask != null;

        private void ChangeStatus()
        {
            if (SelectedTask != null)
            {
                var statuses = new[] { "Новая", "В работе", "На проверке", "Завершена" };
                var currentIndex = Array.IndexOf(statuses, SelectedTask.Status);
                var nextIndex = (currentIndex + 1) % statuses.Length;
                SelectedTask.Status = statuses[nextIndex];
            }
        }

        private bool CanChangeStatus() => SelectedTask != null;

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
