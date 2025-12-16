using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.PaymentGateway
{
    [ExcludeFromCodeCoverage]
    public class VnpayPaymentGateway : IPaymentGateway
    {
        private readonly VnpayPayment _inner = new VnpayPayment();

        public string CreatePaymentUrl(double amount, string orderInfo, int bookingId)
            => _inner.CreatePaymentUrl(amount, orderInfo, bookingId);
    }
}
