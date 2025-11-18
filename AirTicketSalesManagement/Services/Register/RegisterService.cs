using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Register
{
    public class RegisterService : IRegisterService
    {
        private readonly AirTicketDbContext _context;

        public RegisterService(AirTicketDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Taikhoans.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> CreateCustomerAsync(string name, string email, string password)
        {
            try
            {
                string hashPass = BCrypt.Net.BCrypt.HashPassword(password);
                var customer = new Khachhang { HoTenKh = name };
                var account = new Taikhoan
                {
                    Email = email,
                    MatKhau = hashPass,
                    VaiTro = "Khách hàng",
                    MaKhNavigation = customer
                };
                _context.Khachhangs.Add(customer);
                _context.Taikhoans.Add(account);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
