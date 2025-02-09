using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TwoOkNotes.Model;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.Services
{
    public class TimerHandler
    {
        private readonly DispatcherTimer _timer;

        public TimerHandler(TimeSpan interval, EventHandler tickHandler)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = interval;
            _timer.Tick += tickHandler;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
    }
}
