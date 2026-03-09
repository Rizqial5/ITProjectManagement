using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Models.Workspace;
using System.Reflection.Emit;

namespace ProjectManagement.App.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<GithubAuth> GithubAuths { get; set; }
        public DbSet<GithubRepo> GithubRepos { get; set; }
        public DbSet<GithubRepoConnected> GithubRepoConnecteds { get; set; }
        public DbSet<GithubCommit> GithubCommits { get; set; }
        public DbSet<StatusKeyword> StatusKeywords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default keywords
            builder.Entity<StatusKeyword>().HasData(
                new StatusKeyword { Id = 1, Keyword = "#done", TargetStatus = Status.Done, IsActive = true, Description = "Ubah status task menjadi Done" },
                new StatusKeyword { Id = 2, Keyword = "#closes", TargetStatus = Status.Done, IsActive = true, Description = "Ubah status task menjadi Done" },
                new StatusKeyword { Id = 3, Keyword = "#fixes", TargetStatus = Status.Done, IsActive = true, Description = "Ubah status task menjadi Done" },
                new StatusKeyword { Id = 4, Keyword = "#inprogress", TargetStatus = Status.InProgress, IsActive = true, Description = "Ubah status task menjadi InProgress" },
                new StatusKeyword { Id = 5, Keyword = "#todo", TargetStatus = Status.ToDo, IsActive = true, Description = "Ubah status task menjadi ToDo" }
            );


            //GithubCommits
            builder.Entity<GithubCommit>().HasIndex(c=> c.Sha).IsUnique();


            builder.Entity<GithubRepo>()
                .Property(r => r.RepoId)
                .ValueGeneratedNever(); // <-

            builder.Entity<GithubRepoConnected>()
                .HasKey(c => new { c.ProjectId, c.UserId, c.RepoId });

            builder.Entity<GithubRepoConnected>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            builder.Entity<GithubRepoConnected>()
                .HasOne(c => c.Project)
                .WithMany(p=> p.GithubRepoConnecteds)
                .HasForeignKey(c => c.ProjectId);

            builder.Entity<GithubRepoConnected>()
                .HasOne(c => c.Repo)
                .WithMany()
                .HasForeignKey(c => c.RepoId);
        }
    }
}
