using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;

namespace ProjectManagement.App.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync(int projectId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetAsync(int projectId, int taskId)
        {
            return await _dbContext.TaskItems.Include(i=> i.Project)
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        }

        public async Task AddAsync(int projectId, TaskItem task)
        {
            task.ProjectId = projectId;
            await _dbContext.TaskItems.AddAsync(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int projectId, TaskItem task)
        {
            var existing = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == task.Id);
            if (existing == null) return false;

            existing.Title = task.Title;
            existing.Status = task.Status;
            existing.AssignedUserId = task.AssignedUserId;
            // Tambahkan property lain jika ada

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int projectId, int taskId)
        {
            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
            if (task == null) return false;
            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommitDto>> GetAllIntegratedCommitAsync(int projectId)
        {
            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i=> i.Repo)
                    .ThenInclude(i=> i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var commitData = commmitQuery.Repo.Commits.Where(i=> i.isAssignedTask).Select(i => new CommitDto
            {
                AuthorName = i.AuthorName,
                CommitDate = i.CommitDate,
                Message = i.Message,
                Id = i.Id,
                IsIntegrated = i.isAssignedTask
            });

            return commitData;

                
        }

        public async Task<IEnumerable<CommitDto>> GetAllCommitAsync(int projectId)
        {
            var commmitQuery = await _dbContext.GithubRepoConnecteds
               .Include(i => i.Repo)
                   .ThenInclude(i => i!.Commits)
               .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var commitData = commmitQuery.Repo.Commits.Where(i => !i.isAssignedTask).Select(i => new CommitDto
            {
                AuthorName = i.AuthorName,
                CommitDate = i.CommitDate,
                Message = i.Message,
                Sha = i.Sha,
                Id = i.Id,
                RepoId = i.RepoId,
                IsIntegrated = i.isAssignedTask
            }).OrderByDescending(i => i.CommitDate);

            return commitData;

        }

        public async Task<ResponseResultDto> ConnectCommitToTaskAsync(int repoId, int commitId, int taskId)
        {
            var checkExistingCommit = await _dbContext.GithubCommits.FirstOrDefaultAsync(i => i.RepoId == repoId && i.Id == commitId);

            ArgumentNullException.ThrowIfNull(checkExistingCommit);

            checkExistingCommit.TaskId = taskId;

            await _dbContext.SaveChangesAsync();

            return new()
            {
                Success = true,
                Message = string.Empty
            };

            
        }

        public async Task<int> GetTotalIntegratedCommit(int projectId)
        {
            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i => i.Repo)
                    .ThenInclude(i => i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var totalCommit = commmitQuery?.Repo?.Commits.Where(i => i.isAssignedTask).Count();
            

            return totalCommit.Value;
        }
    }
}