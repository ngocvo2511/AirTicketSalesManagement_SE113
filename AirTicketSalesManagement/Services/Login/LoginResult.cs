using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Login
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Role { get; set; }
        public string? Error { get; set; }
        public int? AccountId { get; set; }
        public int? CustomerId { get; set; }
        public int? StaffId { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
    }
}
