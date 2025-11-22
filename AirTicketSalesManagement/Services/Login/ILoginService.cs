using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Login
{
    public interface ILoginService
    {
        Task<LoginResult> LoginAsync(string email, string password);
    }
}
