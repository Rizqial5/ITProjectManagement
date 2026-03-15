using ProjectManagement.App.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Project
{
    public class AddProjectMemberDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please select a user")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public ProjectRole Role { get; set; } = ProjectRole.Developer;

        public string? ProjectTitle { get; set; }
    }
}
