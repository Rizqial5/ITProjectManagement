using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models
{
    public enum NotificationType
    {
        General = 1,
        WorkspaceInvite = 2,
        ProjectInvite = 3,
        TaskAssigned = 4
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RecipientUserId { get; set; } = string.Empty;
        public ApplicationUser Recipient { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(250)]
        public string Message { get; set; } = string.Empty;

        public string? ActionUrl { get; set; }
        public string? DeclineUrl { get; set; }
        public string? IconCssClass { get; set; } // e.g., "fas fa-user-plus"

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Link to other entities if needed
        public int? RelatedInviteId { get; set; } 

        public NotificationType Type { get; set; } = NotificationType.General;
    }
}

