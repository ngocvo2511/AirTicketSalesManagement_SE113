using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.ViewModel.Customer;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AirTicketSalesManagement.View.Customer
{
    /// <summary>
    /// Interaction logic for BookingHistoryDetailView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class BookingHistoryDetailView : UserControl
    {
        public BookingHistoryDetailView()
        {
            InitializeComponent();
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BookingHistoryDetailViewModel vm)
            {
                await vm.LoadData();
            }
        }
        
    }
}
