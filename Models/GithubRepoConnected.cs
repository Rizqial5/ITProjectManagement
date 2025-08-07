using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagement.App.Models
{
    public class GithubRepoConnected
    {

        public int ProjectId { get; set; }
        public int RepoId { get; set; }

        public string UserId { get; set; } = default!;

        public required ApplicationUser User { get; set; }
        public required Project Project { get; set; }
        public required GithubRepo Repo { get; set; }

    }
}
