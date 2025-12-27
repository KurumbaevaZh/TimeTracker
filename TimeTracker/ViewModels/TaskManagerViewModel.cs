using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    internal class TaskManagerViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private Employee _currentEmployee;

        private List<Models.Task> _allTasks;
        private List<Models.Task> _filteredTasks;
        public List<Models.Task> FilteredTasks
        {
            get => _filteredTasks;
            set => SetProperty(ref _filteredTasks, value);
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

        private string _newTaskTitle;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        private string _newTaskDescription;
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }

        private Project _selectedProjectForNewTask;
        public Project SelectedProjectForNewTask
        {
            get => _selectedProjectForNewTask;
            set => SetProperty(ref _selectedProjectForNewTask, value);
        }

        private Employee _selectedEmployeeForNewTask;
        public Employee SelectedEmployeeForNewTask
        {
            get => _selectedEmployeeForNewTask;
            set => SetProperty(ref _selectedEmployeeForNewTask, value);
        }

        private string _selectedStatusForNewTask = "Новая";
        public string SelectedStatusForNewTask
        {
            get => _selectedStatusForNewTask;
            set => SetProperty(ref _selectedStatusForNewTask, value);
        }

        public List<string> StatusFilters { get; } = new List<string>
        {
            "Все", "Новая", "В работе", "На проверке", "Завершена"
        };

        public List<string> StatusesForNewTask { get; } = new List<string>
        {
            "Новая", "В работе", "На проверке", "Завершена"
        };

        public RelayCommand LoadDataCommand { get; }
        public RelayCommand CreateTaskCommand { get; }
        public RelayCommand EditTaskCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand ChangeStatusCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        public TaskManagerViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            LoadDataCommand = new RelayCommand(LoadData);
            CreateTaskCommand = new RelayCommand(CreateTask, CanCreateTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            ChangeStatusCommand = new RelayCommand(ChangeStatus, CanChangeStatus);
            ClearFormCommand = new RelayCommand(ClearForm);

            LoadData();

            Console.WriteLine($"Загружено сотрудников: {Employees?.Count ?? 0}");
            Console.WriteLine($"Загружено проектов: {Projects?.Count ?? 0}");

            if (Employees != null)
            {
                foreach (var emp in Employees.Take(5))
                {
                    Console.WriteLine($"Сотрудник: {emp.Id} - {emp.FirstName} {emp.LastName}");
                }
            }
        }

        private void LoadData()
        {
            try
            {
                _allTasks = _context.Tasks.ToList();
                FilteredTasks = _allTasks;
                Projects = _context.Projects.ToList();
                Employees = _context.Employees.ToList();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void FilterTasks()
        {
            if (_allTasks == null) return;

            var filtered = _allTasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedProjectFilter != null)
            {
                filtered = filtered.Where(t => t.ProjectId == SelectedProjectFilter.Id);
            }

            if (SelectedStatusFilter != "Все")
            {
                filtered = filtered.Where(t => t.Status == SelectedStatusFilter);
            }

            FilteredTasks = filtered.ToList();
        }

        private bool CanCreateTask()
        {
            return !string.IsNullOrWhiteSpace(NewTaskTitle) &&
                   SelectedEmployeeForNewTask != null &&
                   SelectedProjectForNewTask != null &&
                   !string.IsNullOrWhiteSpace(SelectedStatusForNewTask);
        }

        private void CreateTask()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTaskTitle))
                {
                    MessageBox.Show("Введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedEmployeeForNewTask == null)
                {
                    MessageBox.Show("Выберите исполнителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedProjectForNewTask == null)
                {
                    MessageBox.Show("Выберите проект", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                var newTask = new Models.Task
                {
                    Title = NewTaskTitle.Trim(),
                    Description = NewTaskDescription?.Trim(),
                    Status = SelectedStatusForNewTask,
                    ProjectId = SelectedProjectForNewTask.Id,       
                    AssignedTo = SelectedEmployeeForNewTask.Id,     
                    AssigneeId = SelectedEmployeeForNewTask.Id      
                };

                _context.Tasks.Add(newTask);
                _context.SaveChanges();
                LoadData();
                ClearForm();
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = "Ошибка создания задачи:\n";

                if (dbEx.InnerException is PostgresException pgEx)
                {
                    errorMessage += $"Код ошибки: {pgEx.SqlState}\n";
                    errorMessage += $"Сообщение: {pgEx.MessageText}\n";

                    if (pgEx.SqlState == "23503")
                    {
                        errorMessage += "\nПРОБЛЕМА С ВНЕШНИМ КЛЮЧОМ!\n";
                        errorMessage += "Проверьте, что:\n";
                        errorMessage += $"1. Проект с ID={SelectedProjectForNewTask?.Id} существует\n";
                        errorMessage += $"2. Сотрудник с ID={SelectedEmployeeForNewTask?.Id} существует\n";

                        errorMessage += "\nТекущие данные в БД:\n";

                        try
                        {
                            var projects = _context.Projects.Select(p => p.Id).ToList();
                            var employees = _context.Employees.Select(e => e.Id).ToList();

                            errorMessage += $"Проекты (ID): {string.Join(", ", projects)}\n";
                            errorMessage += $"Сотрудники (ID): {string.Join(", ", employees)}\n";
                        }
                        catch { }
                    }
                }

                MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EditTask()
        {
            if (SelectedTask != null)
            {
                NewTaskTitle = SelectedTask.Title;
                NewTaskDescription = SelectedTask.Description;
                SelectedStatusForNewTask = SelectedTask.Status;
                SelectedProjectForNewTask = Projects.FirstOrDefault(p => p.Id == SelectedTask.ProjectId);
                SelectedEmployeeForNewTask = Employees.FirstOrDefault(e => e.Id == SelectedTask.AssignedTo);

                MessageBox.Show($"Редактирование задачи: {SelectedTask.Title}\nЗаполните форму и нажмите 'Создать задачу' для сохранения изменений");
            }
        }

        private bool CanEditTask() => SelectedTask != null;

        private void DeleteTask()
        {
            if (SelectedTask != null && MessageBox.Show($"Удалить задачу '{SelectedTask.Title}'?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Tasks.Remove(SelectedTask);
                    _context.SaveChanges();

                    LoadData();

                    MessageBox.Show($"Задача '{SelectedTask.Title}' удалена");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления задачи: " + ex.Message);
                }
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

                try
                {
                    _context.SaveChanges();
                    FilterTasks();

                    MessageBox.Show($"Статус задачи '{SelectedTask.Title}' изменен на '{SelectedTask.Status}'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения статуса: " + ex.Message);
                }
            }
        }

        private bool CanChangeStatus() => SelectedTask != null;

        private void ClearForm()
        {
            NewTaskTitle = "";
            NewTaskDescription = "";
            SelectedStatusForNewTask = "Новая";
            SelectedProjectForNewTask = null;
            SelectedEmployeeForNewTask = null;
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
