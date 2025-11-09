using ProjectManagement.App.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Task
{
    public class CreateTaskDto
    {
        public int ProjectID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public Status Status { get; set; } = Status.ToDo;

        [Required]
        [Display(Name = "Target Date")]
        public DateTime TargetDate { get; set; } = DateTime.UtcNow;

        public string ProjectTitle { get; set; }
    }
}
