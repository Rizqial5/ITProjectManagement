using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Models.Workspace
{
    /// <summary>
    /// Model untuk menjembatani relasi Many-to-Many antara User dan Project.
    /// Serta menyimpan Role akses di tingkat Project.
    /// </summary>
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public ProjectRole Role { get; set; } = ProjectRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
