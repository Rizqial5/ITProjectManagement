using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relasi: 1 Project memiliki banyak TaskItem
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
