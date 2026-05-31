using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;

namespace ProjectManagement.App.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(CreateProjectDto project, int workspaceId)
        {
            if (string.IsNullOrEmpty(project.ProjectOwnerUserId))
                throw new ArgumentException("ProjectOwnerUserId is required.");

            var newProject = new Project
            {
                Name = project.Title ?? "Untitled Project",
                Description = project.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                EndDate = project.EndDate ?? DateTime.UtcNow.AddMonths(1),
                ProjectOwnerUserId = project.ProjectOwnerUserId,
                WorkspaceId = workspaceId // Link to workspace
            };

            await _dbContext.Projects.AddAsync(newProject);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId, int workspaceId, string userId)
        {
            // Strict workspace filter and authorization
            var isAuthorized = await IsUserAuthorizedAsync(projectId, workspaceId, userId);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized access" };
            }

            var existData = await _dbContext.GithubRepoConnecteds
                .Include(c => c.Repo).ThenInclude(r => r!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected && i.Project.WorkspaceId == workspaceId);

            if (existData == null || existData.Repo == null)
            {
                return new()
                {
                    Success = false,
                    Message = "Data or Repo is Not exists"
                };
            }

            var repoDto = new GitHubRepoDto
            {
                Name = existData.Repo.RepoName,
                RepoId = existData.Repo.RepoId,
                Html_Url = existData.Repo.RepoUrl,
                Commits = existData.Repo.Commits

            };

            return new()
            {
                Success = true,
                Data = repoDto

            };
        }


        public async Task<ResponseResultDto> ConnectRepo(string userId, int projectId, int workspaceId, GitHubRepoDto githubRepoDto)
        {
            // Strict workspace filter and authorization
            var isAuthorized = await IsUserAuthorizedAsync(projectId, workspaceId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized: Only Owner or Manager can connect repository." };
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var newRepo = new GithubRepo
                    {
                        RepoId = githubRepoDto.RepoId,
                        RepoName = githubRepoDto.Name,
                        RepoUrl = githubRepoDto.Html_Url,
                    };

                    if (!_dbContext.GithubRepos.Any(i => i.RepoId == githubRepoDto.RepoId))
                    {
                        await _dbContext.GithubRepos.AddAsync(newRepo);
                    }

                    var existingConnected = await _dbContext.GithubRepoConnecteds.FirstOrDefaultAsync((i => i.RepoId == githubRepoDto.RepoId
                    && i.ProjectId == projectId));

                    if (existingConnected != null)
                    {
                        existingConnected.Connected = true;
                    }
                    else
                    {
                        var newGithubConnected = new GithubRepoConnected
                        {
                            ProjectId = projectId,
                            RepoId = newRepo.RepoId,
                            UserId = userId,
                            Connected = true,
                            ConnectedDate = DateTime.UtcNow,

                        };

                        await _dbContext.GithubRepoConnecteds.AddAsync(newGithubConnected);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ResponseResultDto()
                    {
                        Success = true,
                        Message = "Repo succesfully connected"
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ResponseResultDto { Success = false, Message = ex.Message };
                }
            });
        }

        public async Task<bool> DeleteAsync(int[] id, int workspaceId, string userId)
        {
            // Only Owner can delete projects, and must be within the correct workspace
            var projectSelected = await _dbContext.Projects
                .Where(i => id.Contains(i.Id) && i.ProjectOwnerUserId == userId && i.WorkspaceId == workspaceId)
                .ToListAsync();

            if (projectSelected.Any())
            {
                _dbContext.Projects.RemoveRange(projectSelected);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ResponseResultDto> DisconnectRepo(string userId, int projectId, int workspaceId)
        {
            // Strict workspace filter and authorization
            var isAuthorized = await IsUserAuthorizedAsync(projectId, workspaceId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized: Only Owner or Manager can disconnect repository." };
            }

            try
            {
                var existData = await _dbContext.GithubRepoConnecteds
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected && i.Project.WorkspaceId == workspaceId);

                if (existData == null)
                {
                    return new()
                    {
                        Success = false,
                        Message = "No connected repository found for this project."
                    };
                }

                existData.Connected = false;
                existData.DisconnectedDate = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return new()
                {
                    Success = true,
                    Message = "Project has succesfully disconencted"
                };
            }
            catch (Exception ex)
            {
                return new() { Success = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<Project>> GetAllAsync(int workspaceId, string userId)
        {
            // Only fetch projects belonging to this workspace where user is involved
            return await _dbContext.Projects
                .Include(i => i.Tasks)
                .Include(i => i.GithubRepoConnecteds)
                .Where(p => p.WorkspaceId == workspaceId && (p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))
                .ToListAsync();
        }

        public async Task<Project?> GetAsync(int id, int workspaceId, string userId)
        {
            // Strict project fetch by ID AND WorkspaceID
            return await _dbContext.Projects
                .Include(i => i.Tasks)
                    .ThenInclude(i => i.Commits)
                .Include(i => i.GithubRepoConnecteds)
                .FirstOrDefaultAsync(i => i.Id == id && i.WorkspaceId == workspaceId && (i.ProjectOwnerUserId == userId || i.ProjectMembers.Any(m => m.UserId == userId)));
        }

        public async Task<bool> UpdateAsync(Project project, int workspaceId, string userId)
        {
            // Strict workspace filter and authorization
            var isAuthorized = await IsUserAuthorizedAsync(project.Id, workspaceId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized) return false;

            var existing = await _dbContext.Projects
                .FirstOrDefaultAsync(i => i.Id == project.Id && i.WorkspaceId == workspaceId);

            if (existing == null) return false;

            existing.Name = project.Name;
            existing.Description = project.Description;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(int workspaceId, string userId, int take)
        {
            // 1. Try to get projects with active tasks (ToDo or InProgress)
            var activeProjectsQuery = _dbContext.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Commits)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(m => m.User)
                .Where(p => p.WorkspaceId == workspaceId && (p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))
                .Where(p => p.Tasks.Any(t => t.Status == Models.Enum.Status.InProgress || t.Status == Models.Enum.Status.ToDo));

            var activeProjectsData = await activeProjectsQuery.ToListAsync();

            // 2. Fallback: If no projects have active tasks, get the most recently created projects in the workspace
            if (!activeProjectsData.Any())
            {
                return await _dbContext.Projects
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.Commits)
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(m => m.User)
                    .Where(p => p.WorkspaceId == workspaceId && (p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(take)
                    .ToListAsync();
            }

            // 3. If we found active projects, sort them by latest activity (commit date or creation date)
            return activeProjectsData
                .Select(p => new
                {
                    Project = p,
                    LatestCommitDate = p.Tasks.SelectMany(t => t.Commits).Any()
                        ? p.Tasks.SelectMany(t => t.Commits).Max(c => c.CommitDate)
                        : p.CreatedAt
                })
                .OrderByDescending(x => x.LatestCommitDate)
                .Take(take)
                .Select(x => x.Project)
                .ToList();
        }

        public async Task<(int TotalProjects, int TotalTasks, int TotalCompletedTasks)> GetDashboardStatsAsync(int workspaceId, string userId)
        {
            var allProjects = await _dbContext.Projects
                .Include(p => p.Tasks)
                .Where(p => p.WorkspaceId == workspaceId && (p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))
                .ToListAsync();

            int totalProjects = allProjects.Count;
            int totalTasks = allProjects.Sum(p => p.Tasks.Count);
            int totalCompletedTasks = allProjects.Sum(p => p.Tasks.Count(t => t.Status == Models.Enum.Status.Done));

            return (totalProjects, totalTasks, totalCompletedTasks);
        }

        public async Task<bool> IsUserAuthorizedAsync(int projectId, int workspaceId, string userId, params ProjectRole[] requiredRoles)
        {
            var project = await _dbContext.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.WorkspaceId == workspaceId);

            if (project == null) return false;

            // Owner always authorized
            if (project.ProjectOwnerUserId == userId) return true;

            if (requiredRoles == null || requiredRoles.Length == 0)
            {
                // Just check if they are a member at all
                return project.ProjectMembers.Any(m => m.UserId == userId);
            }

            return project.ProjectMembers.Any(m => m.UserId == userId && requiredRoles.Contains(m.Role));
        }
    }
}
