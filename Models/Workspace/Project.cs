using ProjectManagement.App.Models.Github;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectManagement.App.Models.Workspace
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } 

        // Relasi: 1 Project memiliki banyak TaskItem
        [JsonIgnore]
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public string? ProjectOwnerUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public ApplicationUser? ProjectOwner { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

        public ICollection<GithubRepoConnected> GithubRepoConnecteds { get; set; }
    }
}
