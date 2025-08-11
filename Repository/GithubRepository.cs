using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Response;

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
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var listCommit = new List<GithubCommit>();

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

                    listCommit.Add(commit);
                }

                await _dbContext.GithubCommits.AddRangeAsync(listCommit);

                await _dbContext.SaveChangesAsync();

                var latesCommitDate = newCommits.Max(c => c.Commit.Author.Date);

                var existingRepo = await _dbContext.GithubRepos.FirstOrDefaultAsync(r => r.RepoId == repoId);

                existingRepo!.LastKnownCommitDate = latesCommitDate;

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new()
                {
                    Success = true,
                    Message = "Commits is Synced"
                };
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();

                return new()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
