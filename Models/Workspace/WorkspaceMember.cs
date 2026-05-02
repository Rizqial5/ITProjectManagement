using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Workspace
{
    public enum WorkspaceRole
    {
        Admin = 1,
        Member = 2
    }

    public class WorkspaceMember
    {
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
