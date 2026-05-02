using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Workspace
{
    public class Workspace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;
        public ApplicationUser Owner { get; set; } = null!;

        // Navigation properties
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
