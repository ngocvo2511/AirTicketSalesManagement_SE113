using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AirTicketSalesManagement.Services.Timer
{
    public class DispatcherTimerService : ITimerService
    {
        private DispatcherTimer? _timer;

        public void Start(TimeSpan duration, Action<TimeSpan> onTick, Action onFinished)
        {
            TimeSpan timeLeft = duration;
            onTick(timeLeft);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) =>
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
                onTick(timeLeft);

                if (timeLeft <= TimeSpan.Zero)
                {
                    Stop();
                    onFinished();
                }
            };
            _timer.Start();
        }

        public void Stop() => _timer?.Stop();
    }

}
