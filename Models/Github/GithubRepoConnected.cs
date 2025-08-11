using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Models.Workspace;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagement.App.Models.Github
{
    public class GithubRepoConnected
    {

        public int ProjectId { get; set; }
        public int RepoId { get; set; }

        public string UserId { get; set; } = default!;

        public ApplicationUser? User { get; set; }
        public Project? Project { get; set; }
        public GithubRepo? Repo { get; set; }

    }
}
