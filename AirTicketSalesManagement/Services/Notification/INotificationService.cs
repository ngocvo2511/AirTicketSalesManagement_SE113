using AirTicketSalesManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Notification
{
    public interface INotificationService
    {
        Task<bool> ShowNotificationAsync(string message, NotificationType type, bool isConfirmation = false);
    }
}
