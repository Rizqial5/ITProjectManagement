using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Github;
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
            return await _dbContext.TaskItems
                .Include(i => i.Project)
                .Include(i => i.Commits)
                    .ThenInclude(c => c.Repo)
                .Include(i => i.AssignedUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        }

        public async Task AddAsync(int projectId, CreateTaskDto task)
        {


            TaskItem taskItem = new()
            {
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                TargetDate = task.TargetDate,
                ProjectId = task.ProjectID,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.TaskItems.AddAsync(taskItem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int projectId, TaskItem task)
        {
            var existing = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == task.Id);
            if (existing == null) return false;

            existing.Description = task.Description;
            existing.Status = task.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.AssignedUserId = task.AssignedUserId;

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

        public async Task<IEnumerable<CommitDto>> GetAllIntegratedCommitAsync(int projectId, int taskId)
        {
            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i=> i.Repo)
                    .ThenInclude(i=> i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var commitData = commmitQuery.Repo.Commits.Where(i=> i.isAssignedTask && i.TaskId == taskId).Select(i => new CommitDto
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

        public async Task<int> GetTotalIntegratedCommit(int projectId, int taskId)
        {
            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i => i.Repo)
                    .ThenInclude(i => i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var totalCommit = commmitQuery?.Repo?.Commits.Where(i => i.isAssignedTask && i.TaskId == taskId).Count();
            

            return totalCommit.Value;
        }

        public async Task<ResponseResultDto> SetTaskStatus(int projectId, int taskId, Status setStatus)
        {
            try
            {
                var checkTask = await _dbContext.TaskItems
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Id == taskId);

                if (checkTask == null) return new() { Success = false, Message = "Task is not found" };

                checkTask.Status = setStatus;

                await _dbContext.SaveChangesAsync();

                return new()
                {
                    Success = true,
                    Message = "Task has been successfully set to done"
                };

            }
            catch (Exception ex)
            {
                return new()
                {
                    Success = false,
                    Message = ex.Message
                };
            }     
        }

        public async Task<IEnumerable<GithubCommit>> GetLinkedCommit(int repoId, int taskId)
        {
            var listCommit = await _dbContext.GithubCommits.Where(i=> i.RepoId == repoId && i.TaskId == taskId).ToListAsync();

            return listCommit;
        }

        public async Task<bool> UpdateDateAsync(TaskItem updatedData)
        {
            try
            {
                var existing = await _dbContext.TaskItems
                    .FirstOrDefaultAsync(t => t.ProjectId == updatedData.ProjectId && t.Id == updatedData.Id);

                if (existing == null) return false;

                existing.UpdatedAt = DateTime.UtcNow;
                existing.TargetDate = updatedData.TargetDate;


                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

        }

        public async Task AddNoteAsync(SaveNotesDto notesDto)
        {
            try
            {
                var existing = await _dbContext.TaskItems
                    .FirstOrDefaultAsync(t => t.ProjectId == notesDto.ProjectId && t.Id == notesDto.TaskId) ?? throw new Exception("Task is not exists in database");

                existing.NotesHtml = notesDto.NotesHtml;


                await _dbContext.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string?> GetNotesAsync(int projectId, int taskId)
        {
            var task = await _dbContext.TaskItems
                .Where(t => t.ProjectId == projectId && t.Id == taskId)
                .Select(t => t.NotesHtml)
                .FirstOrDefaultAsync();

            return task;
        }
    }
}