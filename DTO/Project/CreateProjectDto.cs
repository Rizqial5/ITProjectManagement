using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Project
{
    public class CreateProjectDto
    {

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Target Date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow;

        public string ProjectOwnerUserId { get; set; } = string.Empty; // tambahkan ini
    }
}
