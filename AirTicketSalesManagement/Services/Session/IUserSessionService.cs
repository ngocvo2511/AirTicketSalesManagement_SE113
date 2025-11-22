using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Session
{
    public interface IUserSessionService
    {
        int? CustomerId { get; }
        int? CurrentTicketId { get; set; }
        string? Email { get; }
    }
}
