using EatTogether.API.Models.Infra;
using EatTogether.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EatTogether.Models.Services
{
    /// <summary>每天早上 9:00 自動掃描，對 3 天內到期且未使用的優惠券寄送提醒信</summary>
    public class CouponExpiryNotifyBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CouponExpiryNotifyBackgroundService> _logger;

        public CouponExpiryNotifyBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CouponExpiryNotifyBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 計算距下一個早上 9:00 的等待時間
                var now  = DateTime.Now;
                var next = now.Date.AddHours(9);
                if (now >= next) next = next.AddDays(1);

                var delay = next - now;
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await SendExpiryNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "優惠券到期提醒背景服務發生錯誤");
                }
            }
        }

        private async Task SendExpiryNotificationsAsync()
        {
            using var scope      = _scopeFactory.CreateScope();
            var context          = scope.ServiceProvider
                .GetRequiredService<EatTogether.API.Models.EfModels.EatTogetherDBContext>();
            var emailService     = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now      = DateTime.Now;
            var deadline = now.AddDays(3);

            // 找出 3 天內到期、未停用的優惠券
            var expiringCoupons = await context.Coupons
                .Where(c => !c.IsDisabled && c.EndDate.HasValue
                         && c.EndDate.Value >= now
                         && c.EndDate.Value <= deadline)
                .ToListAsync();

            if (!expiringCoupons.Any()) return;

            var couponIds = expiringCoupons.Select(c => c.Id).ToHashSet();

            // 找出持有這些券且尚未使用的 MemberCoupon
            var memberCoupons = await context.MemberCoupons
                .Include(mc => mc.Member)
                .Include(mc => mc.Coupon)
                .Where(mc => couponIds.Contains(mc.CouponId) && !mc.IsUsed)
                .ToListAsync();

            foreach (var mc in memberCoupons)
            {
                if (string.IsNullOrWhiteSpace(mc.Member?.Email)) continue;

                try
                {
                    var daysLeft = (int)(mc.Coupon.EndDate!.Value - now).TotalDays;
                    await emailService.SendCouponExpiryNotifyAsync(
                        mc.Member.Email, mc.Member.Name,
                        mc.Coupon.Name, mc.Coupon.Code, mc.Coupon.EndDate.Value);

                    _logger.LogInformation(
                        "優惠券到期提醒已寄出：MemberId={mid}, CouponId={cid}",
                        mc.MemberId, mc.CouponId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "寄送優惠券到期提醒失敗：MemberId={mid}, CouponId={cid}",
                        mc.MemberId, mc.CouponId);
                }
            }
        }
    }
}
