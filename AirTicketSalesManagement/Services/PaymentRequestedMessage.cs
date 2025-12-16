using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Diagnostics.CodeAnalysis;

namespace AirTicketSalesManagement.Services
{
    [ExcludeFromCodeCoverage]
    public class PaymentRequestedMessage : ValueChangedMessage<string>
    {
        public PaymentRequestedMessage(string paymentUrl) : base(paymentUrl) { }
    }
}
