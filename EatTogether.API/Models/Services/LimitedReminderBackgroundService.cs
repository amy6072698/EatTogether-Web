namespace EatTogether.API.Models.Services
{
    /// <summary>每天早上 9:00 自動掃描，對明天到期的限定餐點訂閱者寄送提醒信</summary>
    public class LimitedReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LimitedReminderBackgroundService> _logger;

        public LimitedReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<LimitedReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now  = DateTime.Now;
                var next = now.Date.AddHours(9);
                if (now >= next) next = next.AddDays(1);

                await Task.Delay(next - now, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope   = _scopeFactory.CreateScope();
                    var reminderService = scope.ServiceProvider
                        .GetRequiredService<LimitedReminderService>();
                    await reminderService.SendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "限定餐點提醒背景服務發生錯誤");
                }
            }
        }
    }
}
