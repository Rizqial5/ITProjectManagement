using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Workspace
{
    public class WorkspaceSettingsDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<WorkspaceMemberDto> Members { get; set; } = new List<WorkspaceMemberDto>();
        public List<UserWorkspaceDto> UserWorkspaces { get; set; } = new List<UserWorkspaceDto>();
    }

    public class UserWorkspaceDto
    {
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class WorkspaceMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
