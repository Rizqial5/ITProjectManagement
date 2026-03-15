using Microsoft.AspNetCore.Identity;
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
        public DbSet<ProjectMember> ProjectMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Users
            var user1 = new ApplicationUser { Id = "user-admin", UserName = "admin@project.com", NormalizedUserName = "ADMIN@PROJECT.COM", Email = "admin@project.com", NormalizedEmail = "ADMIN@PROJECT.COM", EmailConfirmed = true, SecurityStamp = "7649d21c-6d8c-4a3e-b873-1f1f50a31693" };
            user1.PasswordHash = "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A=="; // Hashed "Admin123!"

            var user2 = new ApplicationUser { Id = "user-dev", UserName = "dev@project.com", NormalizedUserName = "DEV@PROJECT.COM", Email = "dev@project.com", NormalizedEmail = "DEV@PROJECT.COM", EmailConfirmed = true, SecurityStamp = "8e18b76c-48c0-4384-98c1-f2f270a41704" };
            user2.PasswordHash = "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A=="; // Hashed "Dev123!"

            var user3 = new ApplicationUser { Id = "user-viewer", UserName = "viewer@project.com", NormalizedUserName = "VIEWER@PROJECT.COM", Email = "viewer@project.com", NormalizedEmail = "VIEWER@PROJECT.COM", EmailConfirmed = true, SecurityStamp = "9f29c87d-59d1-4495-a9d2-e3e381b52815" };
            user3.PasswordHash = "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A=="; // Hashed "Viewer123!"

            builder.Entity<ApplicationUser>().HasData(user1, user2, user3);


            // ProjectMember relationship
            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict);


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
