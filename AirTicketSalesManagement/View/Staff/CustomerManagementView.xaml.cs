using AirTicketSalesManagement.ViewModel.Admin;
using System.Windows;
using System.Windows.Controls;

namespace AirTicketSalesManagement.View.Staff
{
    /// <summary>
    /// Interaction logic for CustomerManagementView.xaml
    /// </summary>
    public partial class CustomerManagementView : UserControl
    {
        public CustomerManagementView()
        {
            InitializeComponent();
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel vm)
                await vm.LoadCustomersCommand.ExecuteAsync(null);
        }
    }
}
