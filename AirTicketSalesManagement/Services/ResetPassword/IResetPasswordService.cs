using AirTicketSalesManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.ResetPassword
{
    public interface IResetPasswordService
    {
        Task UpdatePasswordAsync(string email, string newHashedPassword);
    }
}
