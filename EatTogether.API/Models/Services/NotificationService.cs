using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Repositories;

namespace EatTogether.API.Models.Services
{
	public interface INotificationService
	{
		Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int memberId);
		Task MarkAsReadAsync(int notificationId, int memberId);
		Task MarkAllAsReadAsync(int memberId);
	}

	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _repo;

		public NotificationService(INotificationRepository repo)
		{
			_repo = repo;
		}

		public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int memberId)
		{
			var notification = await _repo.GetByMemberIdAsync(memberId);

			return notification.Select(n => new NotificationDto
			{
				Id = n.Id,
				ArticleId = n.ArticleId,
				Title = n.Title,
				IsRead = n.IsRead,
				CreatedAt = n.CreatedAt
			});
		}

		public async Task MarkAsReadAsync(int notificationId, int memberId)
		{
			await _repo.MarkAsReadAsync(notificationId, memberId);
		}

		public async Task MarkAllAsReadAsync(int memberId)
		{
			await _repo.MarkAllAsReadAsync(memberId);
		}

	}
}
