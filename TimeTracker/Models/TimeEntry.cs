using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    internal class TimeEntry
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Duration { get; set; } 

        public int EmployeeId { get; set; }
        public int TaskId { get; set; }

        public Employee Employee { get; set; }
        public Task Task { get; set; }
    }
}
