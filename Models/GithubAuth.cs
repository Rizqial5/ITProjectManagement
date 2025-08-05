using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagement.App.Models
{
    public class GithubAuth
    {
        
        [Key]
        public int Id { get; set; }

        // Relasi ke ASP.NET Identity User
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }     // Navigasi

        public long GitHubId { get; set; }
        public required string GitHubUsername { get; set; }
        public string? GitHubEmail { get; set; }
        public string? GitHubAvatarUrl { get; set; }

        public string AccessToken { get; set; }
        public string? TokenType { get; set; }
        public string? Scope { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        

    }
}
