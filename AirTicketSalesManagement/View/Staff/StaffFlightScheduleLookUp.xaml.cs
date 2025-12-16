using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace AirTicketSalesManagement.View.Staff
{
    [ExcludeFromCodeCoverage]
    public partial class StaffFlightScheduleLookUp : UserControl
    {
        public StaffFlightScheduleLookUp()
        {
            InitializeComponent();
            // Optionally, you can set DataContext here instead of using StaticResource:
            // this.DataContext = new AirTicketSalesManagement.ViewModel.Staff.StaffFlightScheduleLookUpVM();
        }
    }
}