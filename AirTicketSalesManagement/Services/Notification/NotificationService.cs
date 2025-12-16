using AirTicketSalesManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Notification
{
    [ExcludeFromCodeCoverage]
    public class NotificationService : INotificationService
    {
        private readonly NotificationViewModel _notificationViewModel;
        public NotificationViewModel ViewModel => _notificationViewModel;
        public NotificationService(NotificationViewModel notificationViewModel)
        {
            _notificationViewModel = notificationViewModel;
        }

        public Task<bool> ShowNotificationAsync(string message, NotificationType type, bool isConfirmation = false)
        {
            return _notificationViewModel.ShowNotificationAsync(message, type, isConfirmation);
        }
    }
}
