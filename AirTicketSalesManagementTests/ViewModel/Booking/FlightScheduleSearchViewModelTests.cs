using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.ViewModel.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests.ViewModel.Booking
{
    [TestFixture]
    public class FlightScheduleSearchViewModelTests
    {
        [TestFixture]
        public class SelectTicketClassTests
        {
            private FlightScheduleSearchViewModel _viewModel;

            [SetUp]
            public void SetUp()
            {
                _viewModel = new FlightScheduleSearchViewModel();
            }

            [Test]
            public async Task SelectTicketClass_NullSelection_DoesNothing()
            {
                // Act & Assert: Không exception
                var method = typeof(FlightScheduleSearchViewModel)
                    .GetMethod("SelectTicketClass", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.DoesNotThrowAsync(async () =>
                    await (Task)method.Invoke(_viewModel, new object[] { null })
                );
            }

            [Test]
            public async Task SelectTicketClass_NullTicketClass_DoesNothing()
            {
                var selection = new ThongTinChuyenBayDuocChon
                {
                    TicketClass = null,
                    Flight = new KQTraCuuChuyenBayMoRong()
                };
                var method = typeof(FlightScheduleSearchViewModel)
                    .GetMethod("SelectTicketClass", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.DoesNotThrowAsync(async () =>
                    await (Task)method.Invoke(_viewModel, new object[] { selection })
                );
            }

            [Test]
            public async Task SelectTicketClass_NullFlight_DoesNothing()
            {
                var selection = new ThongTinChuyenBayDuocChon
                {
                    TicketClass = new HangVe(),
                    Flight = null
                };
                var method = typeof(FlightScheduleSearchViewModel)
                    .GetMethod("SelectTicketClass", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.DoesNotThrowAsync(async () =>
                    await (Task)method.Invoke(_viewModel, new object[] { selection })
                );
            }
        }
    }
}
