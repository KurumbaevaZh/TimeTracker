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
    /// Логика взаимодействия для TaskManagerWindow.xaml
    /// </summary>
    public partial class TaskManagerWindow : Window
    {
        public TaskManagerWindow(Employee currentEmployee)
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            DataContext = new TaskManagerViewModel(dbContext, currentEmployee);
        }
    }
}
