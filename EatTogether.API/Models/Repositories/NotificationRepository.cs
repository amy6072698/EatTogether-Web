using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly EatTogetherDBContext _context;

		public NotificationRepository(EatTogetherDBContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<UserNotification>> GetByMemberIdAsync(int memberId)
		{
			return await _context.UserNotifications
				.Where(n => n.MemberId == memberId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}


		public async Task MarkAsReadAsync(int notificationId, int memberId)
		{
			var notification = await _context.UserNotifications
				.FirstOrDefaultAsync(n => n.Id == notificationId && n.MemberId == memberId);

			if (notification == null) return;

			notification.IsRead = true;
			await _context.SaveChangesAsync();

		}

		public async Task MarkAllAsReadAsync(int memberId)
		{
			var unread = await _context.UserNotifications
				.Where(n => n.MemberId == memberId && !n.IsRead)
				.ToListAsync();

			unread.ForEach(n => n.IsRead = true);
			await _context.SaveChangesAsync();

		}

	}
}
