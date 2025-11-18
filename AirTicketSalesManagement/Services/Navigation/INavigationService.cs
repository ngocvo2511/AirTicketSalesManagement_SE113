using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Navigation
{
    public interface INavigationService
    {
        void OpenCustomerWindow();
        void OpenStaffWindow();
        void OpenAdminWindow();
    }

}
