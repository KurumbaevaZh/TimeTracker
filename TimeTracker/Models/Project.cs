using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

        public int ManagerId { get; set; }
        public int DepartmentId { get; set; }

        public Employee Manager { get; set; }
        public Department Department { get; set; }
        public ICollection<Task> Tasks { get; set; }
    }
}
