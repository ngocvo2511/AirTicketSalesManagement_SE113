using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Diagnostics.CodeAnalysis;

namespace AirTicketSalesManagement.Messages
{
    [ExcludeFromCodeCoverage]
    public class WebViewClearCacheMessage : ValueChangedMessage<bool>
    {
        public WebViewClearCacheMessage() : base(true) { }
    }
} 