using AirTicketSalesManagement.Data;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.ResetPassword
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly AirTicketDbContext _context;
        public ResetPasswordService(AirTicketDbContext context)
        {
            _context = context;
        }
        public async Task UpdatePasswordAsync(string email, string Password)
        {
            var user = _context.Taikhoans.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                user.MatKhau = BCrypt.Net.BCrypt.HashPassword(Password);
                await _context.SaveChangesAsync();
            }
        }
    }
}
