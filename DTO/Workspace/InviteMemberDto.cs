using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Workspace
{
    public class InviteMemberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
