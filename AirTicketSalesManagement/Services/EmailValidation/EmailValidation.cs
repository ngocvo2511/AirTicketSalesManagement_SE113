using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.EmailValidation
{
    public class EmailValidation : IEmailValidation
    {
        public bool IsValid(string email)
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
