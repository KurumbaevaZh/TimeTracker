using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;

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

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public RelayCommand LoginCommand { get; private set; }
        public event Action Done;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void OnLogin()
        {
            try
            {
                var employee = await _authService.AuthenticateAsync(Email, Password);

                if (employee != null)
                {
                    ErrorMessage = string.Empty;
                    MessageBox.Show($"Добро пожаловать, {employee.FirstName}!", "Успешный вход");
                    Done?.Invoke();
                }
                else
                {
                    ErrorMessage = "Неверный email или пароль";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка подключения к базе данных";
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
