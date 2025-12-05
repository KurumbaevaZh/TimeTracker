using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public int CreatedBy { get; set; }

        public Employee Creator { get; set; }
    }
}
