using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Models.ReportModel
{
    [ExcludeFromCodeCoverage]
    public class ReportSummaryModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalFlights { get; set; }
        public int TotalTicketsSold { get; set; }
    }
}
