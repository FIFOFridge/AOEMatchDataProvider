using AOEMatchDataProvider.Events.Services.InactiveDetectionService;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AOEMatchDataProvider.Services
{
    public sealed class InactiveDetectionService : IDisposable
    {
        IEventAggregator EventAggregator { get; }

        Timer inactivityTimer;

        public InactiveDetectionService(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            ResetTimer();
        }

        public void ResetTimer()
        {
            if(inactivityTimer != null)
            {
                inactivityTimer.Stop();
                inactivityTimer.Dispose();
            }

            inactivityTimer = new Timer
            {
                AutoReset = false,
                Interval = 1000 * 60 * 10 //10 minutes
            };
            inactivityTimer.Elapsed += InactivityTimer_Elapsed;
        }

        private void InactivityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventAggregator.GetEvent<AppInactivityEvent>().Publish();
        }

        public void Dispose()
        {
            if (inactivityTimer != null)
            {
                inactivityTimer.Stop();
                inactivityTimer.Dispose();
            }
        }
    }
}
