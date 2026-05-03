using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Workspace
{
    public enum InviteStatus
    {
        Pending = 1,
        Accepted = 2,
        Expired = 3,
        Canceled = 4,
        Declined = 5
    }

    public class WorkspaceInvite
    {
        [Key]
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        [Required]
        public string InviterUserId { get; set; } = string.Empty;
        public ApplicationUser Inviter { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string InviteeEmail { get; set; } = string.Empty;

        // The user who accepted the invite, if they are already registered
        public string? InviteeId { get; set; }
        public ApplicationUser? Invitee { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public InviteStatus Status { get; set; } = InviteStatus.Pending;
    }
}
