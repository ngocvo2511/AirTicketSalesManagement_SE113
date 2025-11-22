using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.PaymentGateway
{
    public interface IPaymentGateway
    {
        string CreatePaymentUrl(double amount, string orderInfo, int bookingId);
    }
}
