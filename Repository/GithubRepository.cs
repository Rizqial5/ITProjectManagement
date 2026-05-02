using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Response;
using System.Text.RegularExpressions;

namespace ProjectManagement.App.Repository
{
    public class GithubRepository : IGithubRepository
    {
        private readonly AppDbContext _dbContext;

        public GithubRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseResultDto> InsertGithubCommit(List<GithubCommitApiResponse> newCommits, int repoId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    try
                    {
                        var listCommit = new List<GithubCommit>();
                        var taskRegex = new Regex(@"#(\d+)");

                        // Ambil semua keyword aktif dari DB
                        var activeKeywords = await _dbContext.StatusKeywords
                            .Where(k => k.IsActive)
                            .ToListAsync();

                        foreach (var item in newCommits)
                        {
                            var commit = new GithubCommit()
                            {
                                Sha = item.Sha,
                                Message = item.Commit.Message,
                                AuthorName = item.Commit.Author.Name,
                                AuthorEmail = item.Commit.Author.Email,
                                CommitDate = item.Commit.Author.Date,
                                RepoId = repoId,
                            };

                            // 1. Identifikasi Task ID (#angka)
                            var match = taskRegex.Match(item.Commit.Message);
                            if (match.Success && int.TryParse(match.Groups[1].Value, out int taskId))
                            {
                                var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
                                if (task != null)
                                {
                                    commit.TaskId = taskId;

                                    // 2. Scan Status Keyword (Fase 4.2)
                                    foreach (var kw in activeKeywords)
                                    {
                                        if (item.Commit.Message.Contains(kw.Keyword, StringComparison.OrdinalIgnoreCase))
                                        {
                                            task.Status = kw.TargetStatus;
                                            task.UpdatedAt = DateTime.UtcNow;
                                            break; // Ambil keyword pertama yang cocok
                                        }
                                    }
                                }
                            }

                            listCommit.Add(commit);
                        }

                        await _dbContext.GithubCommits.AddRangeAsync(listCommit);
                        await _dbContext.SaveChangesAsync();

                        if (newCommits.Any())
                        {
                            var latesCommitDate = newCommits.Max(c => c.Commit.Author.Date);
                            var existingRepo = await _dbContext.GithubRepos.FirstOrDefaultAsync(r => r.RepoId == repoId);
                            if (existingRepo != null)
                            {
                                existingRepo.LastKnownCommitDate = latesCommitDate;
                            }
                        }

                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new ResponseResultDto()
                        {
                            Success = true,
                            Message = "Commits Synced and Task Statuses Updated via Keywords"
                        };
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw; // Throw supaya ExecutionStrategy bisa melakukan retry jika perlu
                    }
                });
            }
            catch (Exception ex)
            {
                return new ResponseResultDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<GithubCommit>> GetRecentCommitsAsync(string userId, int take)
        {
            // Get RepoIds connected to projects the user is involved in (Owner or Member)
            var userInvolvement = await _dbContext.GithubRepoConnecteds
                .Where(c => c.Connected && (c.Project.ProjectOwnerUserId == userId || c.Project.ProjectMembers.Any(m => m.UserId == userId)))
                .Select(c => new { c.RepoId, ProjectName = c.Project.Name })
                .ToListAsync();

            var connectedRepoIds = userInvolvement.Select(x => x.RepoId).Distinct().ToList();

            if (!connectedRepoIds.Any()) return Enumerable.Empty<GithubCommit>();

            var commits = await _dbContext.GithubCommits
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .Include(c => c.Repo)
                .Where(c => connectedRepoIds.Contains(c.RepoId))
                .OrderByDescending(c => c.CommitDate)
                .Take(take)
                .ToListAsync();

            // Set the ProjectName property for the view
            foreach (var commit in commits)
            {
                commit.ProjectName = commit.Task?.Project?.Name;
                
                if (string.IsNullOrEmpty(commit.ProjectName))
                {
                    var projInfo = userInvolvement.FirstOrDefault(x => x.RepoId == commit.RepoId);
                    commit.ProjectName = projInfo?.ProjectName ?? "Unknown Project";
                }
            }

            return commits;
        }
    }
}
