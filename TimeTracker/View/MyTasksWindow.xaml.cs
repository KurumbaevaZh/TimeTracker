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
    /// Логика взаимодействия для MyTasksWindow.xaml
    /// </summary>
    public partial class MyTasksWindow : Window
    {
        public MyTasksWindow(Employee currentEmployee)
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            DataContext = new MyTasksViewModel(dbContext, currentEmployee);
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
