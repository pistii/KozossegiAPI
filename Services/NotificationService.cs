using KozossegiAPI.Repo;

namespace KozossegiAPI.Services
{
    //Implementation based on:
    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=netcore-cli
    public class NotificationService : IHostedService, IDisposable
    {
        private readonly Timer _timer;
        private readonly IServiceScopeFactory scopeFactory;

        public NotificationService(IServiceScopeFactory scopeFactory)
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0, 0);
            if (scheduledTime.Hour < now.Hour)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            TimeSpan remaining = scheduledTime - now;
            if (remaining <= TimeSpan.Zero)
            {
                scheduledTime.AddDays(1);
                remaining = scheduledTime - now;
            }

            int dueTime = (int)remaining.TotalMilliseconds;
            int period = 24 * 60 * 60 * 1000;

            _timer = new Timer(DoWork, null, dueTime, period);
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                
                await repo.BirthdayNotification();
                await repo.SelectNotification();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
