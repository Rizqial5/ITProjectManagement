using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models
{
    public class GithubRepo
    {

        [Key]
        public int RepoId { get; set; }
        public required string RepoName { get; set; }
        public required string RepoUrl { get; set; }
    }
}
