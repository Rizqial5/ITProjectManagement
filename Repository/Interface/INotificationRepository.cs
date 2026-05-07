using ProjectManagement.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.App.Repository.Interface
{
    public interface INotificationRepository
    {
        Task CreateNotificationAsync(string recipientUserId, string title, string message, string? url = null, string? declineUrl = null, string? iconCssClass = null, int? relatedInviteId = null, NotificationType type = NotificationType.General);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
