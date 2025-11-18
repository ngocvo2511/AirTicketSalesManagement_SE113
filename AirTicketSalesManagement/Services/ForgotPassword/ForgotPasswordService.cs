using AirTicketSalesManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.ForgotPassword
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly AirTicketDbContext _context;
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        public ForgotPasswordService(AirTicketDbContext context)
        {
            _context = context;
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Taikhoans
            .AnyAsync(u => u.Email == email);
        }

        public bool IsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }
    }
}
