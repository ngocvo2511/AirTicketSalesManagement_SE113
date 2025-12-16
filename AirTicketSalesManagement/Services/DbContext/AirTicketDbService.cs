using AirTicketSalesManagement.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.DbContext
{
    [ExcludeFromCodeCoverage]
    public class AirTicketDbService : IAirTicketDbContextService
    {
        public AirTicketDbContext CreateDbContext()
        => new AirTicketDbContext();

    }
}
