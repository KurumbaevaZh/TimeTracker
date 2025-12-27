using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.View
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            var authService = new AuthService(dbContext);
            var viewModel = new LoginViewModel(authService);
            DataContext = viewModel;

            viewModel.LoginSuccess += OnLoginSuccess; ;
        }

        private void OnLoginSuccess(Employee employee)
        {
            this.Hide();

            try
            {
                var mainWindow = new MainWindow(employee);
                mainWindow.Show();
                mainWindow.Closed += (s, e) => this.Close();
            }
            catch
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.Closed += (s, e) => this.Close();
            }

            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }
    }
}
