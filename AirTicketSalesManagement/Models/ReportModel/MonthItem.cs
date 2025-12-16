using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Models.ReportModel
{
    [ExcludeFromCodeCoverage]
    public class MonthItem
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
