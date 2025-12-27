using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeTracker.Models;
using TimeTracker.ViewModels;

namespace TimeTracker.View
{
    /// <summary>
    /// Логика взаимодействия для TeamManagementWindow.xaml
    /// </summary>
    public partial class TeamManagementWindow : Window
    {
        public TeamManagementWindow(Employee currentEmployee)
        {
            InitializeComponent();
            if (currentEmployee.RoleId != 2) 
            {
                MessageBox.Show("Доступ запрещен. Только руководители отделов могут использовать эту функцию.",
                              "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            var dbContext = new AppDbContext();
            DataContext = new TeamManagementViewModel(dbContext, currentEmployee);
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
