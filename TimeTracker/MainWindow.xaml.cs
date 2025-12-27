using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeTracker.Models;
using TimeTracker.View;
using TimeTracker.ViewModels;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Employee _currentEmployee;
        public MainWindow(Employee employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            InitializeWindow();
        }
        public MainWindow()
        {
            InitializeComponent();
            _currentEmployee = new Employee
            {
                Id = 1,
                FirstName = "Тест",
                LastName = "Пользователь",
                Email = "test@test.com",
                Position = "Тестировщик",
                RoleId = 1
            };
            InitializeWindow();
        }


        private void InitializeWindow()
        {
            Title = $"Учет рабочего времени - {_currentEmployee.FirstName} {_currentEmployee.LastName}";
            WelcomeText.Text = $"Добро пожаловать, {_currentEmployee.FirstName}!";
            EmployeeInfo.Text = $"{_currentEmployee.FirstName} {_currentEmployee.LastName}\n{_currentEmployee.Position}";

            if (_currentEmployee.RoleId == 2)
            {
                RegisterButton.Visibility = Visibility.Visible;
                TeamManagementButton.Visibility = Visibility.Visible;
                StatusText.Text = "Руководитель отдела";
            }
            else if (_currentEmployee.RoleId == 3)
            {
                RegisterButton.Visibility = Visibility.Visible;
                StatusText.Text = "Администратор системы";
            }
            else
            {
                StatusText.Text = "Сотрудник";
            }
        }

        private void OpenTimeTracker_Click(object sender, RoutedEventArgs e)
        {
            var timeTrackerWindow = new TimeTrackerWindow(_currentEmployee);
            timeTrackerWindow.Show();
        }

        private void OpenMyTasks_Click(object sender, RoutedEventArgs e)
        {
            var myTasksWindow = new MyTasksWindow(_currentEmployee);
            myTasksWindow.Show();
        }

        private void OpenTaskManager_Click(object sender, RoutedEventArgs e)
        {
            var taskManagerWindow = new TaskManagerWindow(_currentEmployee);
            var dbContext = new AppDbContext();
            taskManagerWindow.DataContext = new TaskManagerViewModel(dbContext, _currentEmployee);
            taskManagerWindow.Show();
        }

        private void OpenReports_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow(_currentEmployee);
            reportsWindow.Show();
        }

        
        private void OpenRegisterWindow_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }

        private void OpenTeamManagement_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEmployee.RoleId == 2) 
            {
                
            }
            else
            {
                MessageBox.Show("Эта функция доступна только руководителям отделов");
            }
        }
    }
}