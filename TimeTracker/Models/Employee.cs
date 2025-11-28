using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    internal class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int DepartmentId { get; set; }
        public int RoleId { get; set; }

        public Department Department { get; set; }
        public Role Role { get; set; }
        public ICollection<Task> AssignedTasks { get; set; }
        public ICollection<TimeEntry> TimeEntries { get; set; }
        public ICollection<Project> ManagedProjects { get; set; }
        public ICollection<Report> CreatedReports { get; set; }
    }
}
