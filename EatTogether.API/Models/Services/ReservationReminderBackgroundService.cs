using EatTogether.API.Models.Infra;
using EatTogether.Models.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EatTogether.Models.Services
{
    /// <summary>每小時掃描一次，對 24hr 後將到的訂位寄送提醒信</summary>
    public class ReservationReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationReminderBackgroundService> _logger;

        public ReservationReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReservationReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "訂位提醒背景服務發生錯誤");
                }

                // 每小時執行一次
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task SendRemindersAsync()
        {
            using var scope  = _scopeFactory.CreateScope();
            var repo         = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now  = DateTime.Now;
            var from = now.AddHours(23);
            var to   = now.AddHours(25);

            var reservations = await repo.GetRemindersAsync(from, to);

            foreach (var r in reservations)
            {
                try
                {
                    await emailService.SendReservationReminderAsync(
                        r.Email!, r.Name, r.BookingNumber, r.ReservationDate);

                    _logger.LogInformation(
                        "已寄送訂位提醒 BookingNumber={bn}", r.BookingNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "寄送訂位提醒失敗 BookingNumber={bn}", r.BookingNumber);
                }
            }
        }
    }
}
