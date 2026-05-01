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

        private async Task<bool> IsAuthorized(int projectId, string userId, params ProjectRole[] roles)
        {
            var project = await _dbContext.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return false;
            
            // Owner always authorized
            if (project.ProjectOwnerUserId == userId) return true;

            if (roles == null || roles.Length == 0)
            {
                // Just check if they are a member at all
                return project.ProjectMembers.Any(m => m.UserId == userId);
            }

            return project.ProjectMembers.Any(m => m.UserId == userId && roles.Contains(m.Role));
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync(int projectId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return Enumerable.Empty<TaskItem>();

            return await _dbContext.TaskItems
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetAsync(int projectId, int taskId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return null;

            return await _dbContext.TaskItems
                .Include(i => i.Project)
                .Include(i => i.Commits)
                    .ThenInclude(c => c.Repo)
                .Include(i => i.AssignedUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        }

        public async Task AddAsync(int projectId, CreateTaskDto task, string userId)
        {
            // Only Owner and Manager can add tasks
            if (!await IsAuthorized(projectId, userId, ProjectRole.Owner, ProjectRole.Manager))
                throw new UnauthorizedAccessException("Only Project Owner or Manager can add tasks.");

            TaskItem taskItem = new()
            {
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                TargetDate = task.TargetDate,
                ProjectId = projectId,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.TaskItems.AddAsync(taskItem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int projectId, TaskItem task, string userId)
        {
            var existing = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == task.Id);
            if (existing == null) return false;

            var isManager = await IsAuthorized(projectId, userId, ProjectRole.Owner, ProjectRole.Manager);

            if (!isManager)
            {
                // If just a member, check if they are assigned to this task
                if (existing.AssignedUserId != userId) return false;

                // Members can only update Status
                existing.Status = task.Status;
            }
            else
            {
                existing.Description = task.Description;
                existing.Status = task.Status;
                existing.AssignedUserId = task.AssignedUserId;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int projectId, int taskId, string userId)
        {
            // Only Owner and Manager can delete tasks
            if (!await IsAuthorized(projectId, userId, ProjectRole.Owner, ProjectRole.Manager))
                return false;

            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
            if (task == null) return false;

            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommitDto>> GetAllIntegratedCommitAsync(int projectId, int taskId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return Enumerable.Empty<CommitDto>();

            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i=> i.Repo)
                    .ThenInclude(i=> i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            if (commmitQuery?.Repo?.Commits == null) return Enumerable.Empty<CommitDto>();

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

        public async Task<IEnumerable<CommitDto>> GetAllCommitAsync(int projectId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return Enumerable.Empty<CommitDto>();

            var commmitQuery = await _dbContext.GithubRepoConnecteds
               .Include(i => i.Repo)
                   .ThenInclude(i => i!.Commits)
               .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            if (commmitQuery?.Repo?.Commits == null) return Enumerable.Empty<CommitDto>();

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

        public async Task<ResponseResultDto> ConnectCommitToTaskAsync(int repoId, int commitId, int taskId, string userId)
        {
            var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return new() { Success = false, Message = "Task not found" };

            if (!await IsAuthorized(task.ProjectId, userId))
                return new() { Success = false, Message = "Unauthorized access" };

            var checkExistingCommit = await _dbContext.GithubCommits.FirstOrDefaultAsync(i => i.RepoId == repoId && i.Id == commitId);
            if (checkExistingCommit == null) return new() { Success = false, Message = "Commit not found" };

            checkExistingCommit.TaskId = taskId;
            await _dbContext.SaveChangesAsync();

            return new() { Success = true, Message = string.Empty };
        }

        public async Task<int> GetTotalIntegratedCommit(int projectId, int taskId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return 0;

            var commmitQuery = await _dbContext.GithubRepoConnecteds
                .Include(i => i.Repo)
                    .ThenInclude(i => i!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

            var totalCommit = commmitQuery?.Repo?.Commits.Where(i => i.isAssignedTask && i.TaskId == taskId).Count();
            
            return totalCommit ?? 0;
        }

        public async Task<ResponseResultDto> SetTaskStatus(int projectId, int taskId, Status setStatus, string userId)
        {
            try
            {
                var checkTask = await _dbContext.TaskItems
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Id == taskId);

                if (checkTask == null) return new() { Success = false, Message = "Task is not found" };

                var isAuthorized = await IsAuthorized(projectId, userId, ProjectRole.Owner, ProjectRole.Manager);
                if (!isAuthorized && checkTask.AssignedUserId != userId)
                {
                    return new() { Success = false, Message = "Unauthorized: You are not assigned to this task." };
                }

                checkTask.Status = setStatus;
                checkTask.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return new()
                {
                    Success = true,
                    Message = "Task status updated successfully"
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

        public async Task<IEnumerable<GithubCommit>> GetLinkedCommit(int repoId, int taskId, string userId)
        {
            var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null || !await IsAuthorized(task.ProjectId, userId))
                return Enumerable.Empty<GithubCommit>();

            return await _dbContext.GithubCommits.Where(i=> i.RepoId == repoId && i.TaskId == taskId).ToListAsync();
        }

        public async Task<bool> UpdateDateAsync(TaskItem updatedData, string userId)
        {
            try
            {
                if (!await IsAuthorized(updatedData.ProjectId, userId, ProjectRole.Owner, ProjectRole.Manager))
                    return false;

                var existing = await _dbContext.TaskItems
                    .FirstOrDefaultAsync(t => t.ProjectId == updatedData.ProjectId && t.Id == updatedData.Id);

                if (existing == null) return false;

                existing.UpdatedAt = DateTime.UtcNow;
                existing.TargetDate = updatedData.TargetDate;


                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task AddNoteAsync(SaveNotesDto notesDto, string userId)
        {
            var existing = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.ProjectId == notesDto.ProjectId && t.Id == notesDto.TaskId);

            if (existing == null) throw new Exception("Task is not exists in database");

            var isAuthorized = await IsAuthorized(notesDto.ProjectId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized && existing.AssignedUserId != userId)
            {
                throw new UnauthorizedAccessException("Unauthorized access to task notes.");
            }

            existing.NotesHtml = notesDto.NotesHtml;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetNotesAsync(int projectId, int taskId, string userId)
        {
            if (!await IsAuthorized(projectId, userId)) return null;

            return await _dbContext.TaskItems
                .Where(t => t.ProjectId == projectId && t.Id == taskId)
                .Select(t => t.NotesHtml)
                .FirstOrDefaultAsync();
        }
    }
}
