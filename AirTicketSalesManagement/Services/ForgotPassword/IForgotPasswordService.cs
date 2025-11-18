using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.ForgotPassword
{
    public interface IForgotPasswordService
    {
        Task<bool> EmailExistsAsync(string email);
        bool IsValid(string email);
    }
}
