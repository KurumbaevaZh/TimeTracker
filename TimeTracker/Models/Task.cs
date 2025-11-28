using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    internal class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public int ProjectId { get; set; }
        public int AssignedTo { get; set; }

        public Project Project { get; set; }
        public Employee Assignee { get; set; }
        public ICollection<TimeEntry> TimeEntries { get; set; }
    }
}
