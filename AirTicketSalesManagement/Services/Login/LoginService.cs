using AirTicketSalesManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly AirTicketDbContext _context;

        public LoginService(AirTicketDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            var user = await _context.Taikhoans
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.MatKhau))
            {
                return new LoginResult { Success = false, Error = "Tài khoản hoặc mật khẩu không hợp lệ" };
            }

            if (user.VaiTro == "Khách hàng")
            {
                var kh = await _context.Khachhangs.FirstOrDefaultAsync(k => k.MaKh == user.MaKh);
                if (kh == null)
                    return new LoginResult { Success = false, Error = "Không tìm thấy thông tin khách hàng" };

                return new LoginResult
                {
                    Success = true,
                    Role = "Khách hàng",
                    AccountId = user.MaTk,
                    CustomerId = user.MaKh,
                    DisplayName = kh.HoTenKh,
                    Email = user.Email
                };
            }

            if (user.VaiTro == "Nhân viên" || user.VaiTro == "Admin")
            {
                var nv = await _context.Nhanviens.FirstOrDefaultAsync(n => n.MaNv == user.MaNv);
                if (nv == null)
                    return new LoginResult { Success = false, Error = "Không tìm thấy thông tin nhân viên" };

                return new LoginResult
                {
                    Success = true,
                    Role = user.VaiTro,
                    AccountId = user.MaTk,
                    StaffId = user.MaNv,
                    DisplayName = nv.HoTenNv,
                    Email = user.Email
                };
            }

            return new LoginResult { Success = false, Error = "Vai trò không hợp lệ" };
        }
    }

}
