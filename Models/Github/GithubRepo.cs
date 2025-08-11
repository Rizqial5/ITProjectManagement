using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Github
{
    public class GithubRepo
    {

        [Key]
        public int RepoId { get; set; }
        public required string RepoName { get; set; }
        public required string RepoUrl { get; set; }

        // Commit
        public ICollection<GithubCommit> Commits { get; set; } = new List<GithubCommit>();
    }
}
