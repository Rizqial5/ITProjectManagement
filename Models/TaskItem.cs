using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.App.Models.Enum;

namespace ProjectManagement.App.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
        public string Description { get; set; }

        public Status Status { get; set; } = Status.ToDo;

        // Foreign Key ke Project
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        //Opsional: Assigned user(akan digunakan di fase 2.2)
        public string? AssignedUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public ApplicationUser? AssignedUser { get; set; }
    }
}
