using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Register
{
    public interface IRegisterService
    {
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> CreateCustomerAsync(string name, string email, string password);
    }
}
