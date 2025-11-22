using AirTicketSalesManagement.View.Admin;
using AirTicketSalesManagement.View.Customer;
using AirTicketSalesManagement.View.Staff;
using AirTicketSalesManagement.ViewModel.Admin;
using AirTicketSalesManagement.ViewModel.Customer;
using AirTicketSalesManagement.ViewModel.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace AirTicketSalesManagement.Services.Navigation
{
    public class NavigationWindowService : INavigationService
    {
        public void OpenCustomerWindow()
        {
            Open(new CustomerWindow { DataContext = new CustomerViewModel() });
        }

        public void OpenStaffWindow()
        {
            Open(new StaffWindow { DataContext = new StaffViewModel() });
        }

        public void OpenAdminWindow()
        {
            Open(new AdminWindow { DataContext = new AdminViewModel() });
        }

        private void Open(Window window)
        {
            var current = Application.Current.MainWindow;
            Application.Current.MainWindow = window;
            current?.Close();

            window.Opacity = 0;
            window.Show();

            window.BeginAnimation(Window.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(270)));
        }
    }

}
