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
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _position;
        public string Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        private List<Department> _departments;
        public List<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public RelayCommand RegisterCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public event Action Done;
        public event Action Cancel;

        public RegisterViewModel(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            RegisterCommand = new RelayCommand(OnRegister, CanRegister);
            CancelCommand = new RelayCommand(OnCancel);
            LoadDepartments();
        }

        private async void LoadDepartments()
        {
            try
            {
                Departments = await _context.Departments.ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки отделов";
            }
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   SelectedDepartment != null &&
                   Password == ConfirmPassword;
        }

        private async void OnRegister()
        {
            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var existingUser = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == Email);

                if (existingUser != null)
                {
                    ErrorMessage = "Пользователь с таким email уже существует";
                    return;
                }

                var newEmployee = new Employee
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = Password,
                    Position = Position,
                    DepartmentId = SelectedDepartment.Id,
                    RoleId = 1, 
                    CreatedAt = DateTime.Now
                };

                await _context.Employees.AddAsync(newEmployee);
                await _context.SaveChangesAsync();

                SuccessMessage = "Регистрация прошла успешно!";
                MessageBox.Show($"Сотрудник {FirstName} {LastName} успешно зарегистрирован", "Успех");

                Done?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при регистрации";
            }
        }

        private void OnCancel()
        {
            Cancel?.Invoke();
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
