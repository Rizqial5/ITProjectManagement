using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Models;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
