using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Session
{
    [ExcludeFromCodeCoverage]
    public class UserSessionService : IUserSessionService
    {
        public int? CustomerId => UserSession.Current.CustomerId;
        public int? CurrentTicketId
        {
            get => UserSession.Current.idVe;
            set => UserSession.Current.idVe = (int)value;
        }
        public string? Email => UserSession.Current.Email;
    }
}
