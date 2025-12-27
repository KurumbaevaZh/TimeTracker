using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.ViewModels
{
    internal class TeamManagementViewModel
    {
        private readonly AppDbContext _context;
        private readonly Employee _currentEmployee; 
        private List<Employee> _teamEmployees;
        public List<Employee> TeamEmployees
        {
            get => _teamEmployees;
            set => SetProperty(ref _teamEmployees, value);
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                SetProperty(ref _selectedEmployee, value);
                UpdateSelectedEmployeeInfo();
            }
        }

        private string _selectedEmployeeFullName;
        public string SelectedEmployeeFullName
        {
            get => _selectedEmployeeFullName;
            set => SetProperty(ref _selectedEmployeeFullName, value);
        }

        private string _selectedEmployeePosition;
        public string SelectedEmployeePosition
        {
            get => _selectedEmployeePosition;
            set => SetProperty(ref _selectedEmployeePosition, value);
        }

        private string _selectedEmployeeEmail;
        public string SelectedEmployeeEmail
        {
            get => _selectedEmployeeEmail;
            set => SetProperty(ref _selectedEmployeeEmail, value);
        }

        private string _selectedEmployeeInitials;
        public string SelectedEmployeeInitials
        {
            get => _selectedEmployeeInitials;
            set => SetProperty(ref _selectedEmployeeInitials, value);
        }

        private string _selectedEmployeeTasksInfo;
        public string SelectedEmployeeTasksInfo
        {
            get => _selectedEmployeeTasksInfo;
            set => SetProperty(ref _selectedEmployeeTasksInfo, value);
        }

        public RelayCommand LoadDataCommand { get; }

        public TeamManagementViewModel(AppDbContext context, Employee currentEmployee)
        {
            _context = context;
            _currentEmployee = currentEmployee;

            LoadDataCommand = new RelayCommand(LoadTeamData);
            LoadTeamData();
        }

        private void LoadTeamData()
        {
            try
            {
                
                TeamEmployees = _context.Employees
                    .Where(e => e.DepartmentId == _currentEmployee.DepartmentId &&
                               e.Id != _currentEmployee.Id) 
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .ToList();

                if (TeamEmployees.Any() && SelectedEmployee == null)
                {
                    SelectedEmployee = TeamEmployees.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников отдела: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateSelectedEmployeeInfo()
        {
            if (SelectedEmployee == null)
            {
                ClearEmployeeInfo();
                return;
            }

            try
            {
                SelectedEmployeeFullName = $"{SelectedEmployee.FirstName} {SelectedEmployee.LastName}";
                SelectedEmployeePosition = SelectedEmployee.Position;
                SelectedEmployeeEmail = SelectedEmployee.Email;

                SelectedEmployeeInitials = GetInitials(SelectedEmployee.FirstName, SelectedEmployee.LastName);

                UpdateEmployeeTasksInfo();
            }
            catch
            {
                SelectedEmployeeTasksInfo = "Ошибка загрузки информации о задачах";
            }
        }

        private string GetInitials(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return "??";

            return $"{firstName[0]}{lastName[0]}".ToUpper();
        }

        private void UpdateEmployeeTasksInfo()
        {
            try
            {
                var employeeTasks = _context.Tasks
                    .Where(t => t.AssignedTo == SelectedEmployee.Id)
                    .ToList();

                int totalTasks = employeeTasks.Count;
                int activeTasks = employeeTasks.Count(t => t.Status != "Завершена");
                int completedTasks = employeeTasks.Count(t => t.Status == "Завершена");

                SelectedEmployeeTasksInfo = $"Задач всего: {totalTasks} • Активных: {activeTasks} • Завершено: {completedTasks}";
            }
            catch (Exception ex)
            {
                SelectedEmployeeTasksInfo = $"Ошибка загрузки задач: {ex.Message}";
            }
        }

        private void ClearEmployeeInfo()
        {
            SelectedEmployeeFullName = "Выберите сотрудника";
            SelectedEmployeePosition = "";
            SelectedEmployeeEmail = "";
            SelectedEmployeeInitials = "??";
            SelectedEmployeeTasksInfo = "";
        }

        #region INotifyPropertyChanged
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
        #endregion
    }
}
