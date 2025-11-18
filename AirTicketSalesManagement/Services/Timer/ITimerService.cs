using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Timer
{
    public interface ITimerService
    {
        void Start(TimeSpan duration, Action<TimeSpan> onTick, Action onFinished);
        void Stop();
    }
}
