using System.Timers;
using Timer = System.Timers.Timer;

namespace KozoskodoAPI.Services
{
    public interface ITimerService
    {
        event EventHandler Elapsed;
        void Start(TimeSpan interval);
        void Stop();
    }

    public class TimerService : ITimerService
    {
        private readonly Timer _timer;

        public event EventHandler Elapsed;

        public TimerService()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start(TimeSpan interval)
        {
            _timer.Interval = interval.TotalMilliseconds;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}
