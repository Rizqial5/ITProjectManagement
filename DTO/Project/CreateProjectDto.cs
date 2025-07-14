using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Project
{
    public class CreateProjectDto
    {

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
