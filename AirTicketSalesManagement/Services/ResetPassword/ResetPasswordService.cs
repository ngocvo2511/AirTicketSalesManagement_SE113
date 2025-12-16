using AirTicketSalesManagement.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.ResetPassword
{
    [ExcludeFromCodeCoverage]
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly AirTicketDbContext _context;
        public ResetPasswordService(AirTicketDbContext context)
        {
            _context = context;
        }
        public async Task UpdatePasswordAsync(string email, string newHashedPassword)
        {
            var user = _context.Taikhoans.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                user.MatKhau = newHashedPassword;
                await _context.SaveChangesAsync();
            }
        }
    }
}
