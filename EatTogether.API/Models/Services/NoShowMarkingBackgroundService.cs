using EatTogether.Models.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EatTogether.Models.Services
{
    /// <summary>每 5 分鐘掃描一次，將逾時 10 分鐘未報到的訂位標記為 No-Show；累積 3 次封鎖訂位權限</summary>
    public class NoShowMarkingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NoShowMarkingBackgroundService> _logger;
        private const int NoShowThreshold = 3;

        public NoShowMarkingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NoShowMarkingBackgroundService> logger)
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
                    await MarkNoShowsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No-Show 標記背景服務發生錯誤");
                }

                // 每 5 分鐘執行一次
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task MarkNoShowsAsync()
        {
            using var scope    = _scopeFactory.CreateScope();
            var reservationRepo = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

            var pendingIds = await reservationRepo.GetPendingNoShowIdsAsync();
            if (!pendingIds.Any()) return;

            foreach (var id in pendingIds)
            {
                var detail = await reservationRepo.GetByIdAsync(id);
                if (detail == null) continue;

                await reservationRepo.MarkNoShowAsync(id);
                _logger.LogInformation("No-Show 標記：ReservationId={id}", id);

                // 有綁定會員才累積 No-Show 次數並判斷是否封鎖
                if (!detail.MemberId.HasValue) continue;

                var noShowCount = await reservationRepo.GetNoShowCountAsync(detail.MemberId.Value);
                if (noShowCount >= NoShowThreshold)
                {
                    // 透過 DbContext 直接更新黑名單（使用 scope 取得 DbContext）
                    var context = scope.ServiceProvider
                        .GetRequiredService<EatTogether.API.Models.EfModels.EatTogetherDBContext>();

                    var member = await context.Members.FindAsync(detail.MemberId.Value);
                    if (member != null && !member.IsBlacklisted)
                    {
                        member.IsBlacklisted    = true;
                        member.BlacklistReason  = $"累積 No-Show {noShowCount} 次，系統自動封鎖";
                        await context.SaveChangesAsync();

                        _logger.LogWarning(
                            "會員 MemberId={mid} 累積 No-Show {count} 次，已自動封鎖訂位",
                            detail.MemberId.Value, noShowCount);
                    }
                }
            }
        }
    }
}
