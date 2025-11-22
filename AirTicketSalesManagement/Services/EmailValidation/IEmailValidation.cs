using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.EmailValidation
{
    public interface IEmailValidation
    {
        bool IsValid(string email);
    }
}
