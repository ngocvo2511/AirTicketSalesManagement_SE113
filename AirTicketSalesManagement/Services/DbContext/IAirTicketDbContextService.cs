using AirTicketSalesManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.DbContext
{
    public interface IAirTicketDbContextService
    {
        AirTicketDbContext CreateDbContext();
    }
}
