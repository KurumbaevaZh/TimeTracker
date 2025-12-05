using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> AuthenticateAsync(string email, string password)
        {
            try
            {                var employee = await _context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == email && e.Password == password);

                return employee;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
