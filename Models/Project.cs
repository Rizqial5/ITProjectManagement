using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public string? ProjectOwnerUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public ApplicationUser? ProjectOwner { get; set; }
    }
}
