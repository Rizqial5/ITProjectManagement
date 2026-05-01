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

        public async Task AddAsync(CreateProjectDto project)
        {
            if (string.IsNullOrEmpty(project.ProjectOwnerUserId))
                throw new ArgumentException("ProjectOwnerUserId is required.");

            var newProject = new Project
            {
                Name = project.Title,
                Description = project.Description,
                CreatedAt = DateTime.UtcNow,
                EndDate = project.EndDate,
                ProjectOwnerUserId = project.ProjectOwnerUserId
            };

            await _dbContext.Projects.AddAsync(newProject);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId, string userId)
        {
            // Only Owner, Manager, Member can see connection status
            var isAuthorized = await IsUserAuthorizedAsync(projectId, userId);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized access" };
            }

            var existData = await _dbContext.GithubRepoConnecteds
                .Include(c => c.Repo).ThenInclude(r => r!.Commits)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

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


        public async Task<ResponseResultDto> ConnectRepo(string userId, int projectId, GitHubRepoDto githubRepoDto)
        {
            // Only Owner or Manager can connect repo
            var isAuthorized = await IsUserAuthorizedAsync(projectId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized: Only Owner or Manager can connect repository." };
            }

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

                return new()
                {
                    Success = true,
                    Message = "Repo succesfully connected"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new() { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> DeleteAsync(int[] id, string userId)
        {
            // Only Owner can delete projects
            var projectSelected = await _dbContext.Projects
                .Where(i => id.Contains(i.Id) && i.ProjectOwnerUserId == userId)
                .ToListAsync();

            if (projectSelected.Any())
            {
                _dbContext.Projects.RemoveRange(projectSelected);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ResponseResultDto> DisconnectRepo(string userId, int projectId)
        {
            // Only Owner or Manager can disconnect repo
            var isAuthorized = await IsUserAuthorizedAsync(projectId, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized)
            {
                return new() { Success = false, Message = "Unauthorized: Only Owner or Manager can disconnect repository." };
            }

            try
            {
                var existData = await _dbContext.GithubRepoConnecteds
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Connected);

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

        public async Task<IEnumerable<Project>> GetAllAsync(string userId)
        {
            return await _dbContext.Projects
                .Include(i => i.Tasks)
                .Include(i => i.GithubRepoConnecteds)
                .Where(p => p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project?> GetAsync(int id, string userId)
        {
            return await _dbContext.Projects
                .Include(i => i.Tasks)
                    .ThenInclude(i => i.Commits)
                .Include(i => i.GithubRepoConnecteds)
                .FirstOrDefaultAsync(i => i.Id == id && (i.ProjectOwnerUserId == userId || i.ProjectMembers.Any(m => m.UserId == userId)));
        }

        public async Task<bool> UpdateAsync(Project project, string userId)
        {
            // Only Owner or Manager can update project details
            var isAuthorized = await IsUserAuthorizedAsync(project.Id, userId, ProjectRole.Owner, ProjectRole.Manager);
            if (!isAuthorized) return false;

            var existing = await _dbContext.Projects
                .FirstOrDefaultAsync(i => i.Id == project.Id);

            if (existing == null) return false;

            existing.Name = project.Name;
            existing.Description = project.Description;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(string userId, int take)
        {
            var activeProjectsData = await _dbContext.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Commits)
                .Where(p => p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId))
                .Where(p => p.Tasks.Any(t => t.Status == Models.Enum.Status.InProgress || t.Status == Models.Enum.Status.ToDo))
                .ToListAsync();

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
                .Select(x => x.Project);
        }

        public async Task<(int TotalProjects, int TotalTasks, int TotalCompletedTasks)> GetDashboardStatsAsync(string userId)
        {
            var allProjects = await _dbContext.Projects
                .Include(p => p.Tasks)
                .Where(p => p.ProjectOwnerUserId == userId || p.ProjectMembers.Any(m => m.UserId == userId))
                .ToListAsync();

            int totalProjects = allProjects.Count;
            int totalTasks = allProjects.Sum(p => p.Tasks.Count);
            int totalCompletedTasks = allProjects.Sum(p => p.Tasks.Count(t => t.Status == Models.Enum.Status.Done));

            return (totalProjects, totalTasks, totalCompletedTasks);
        }

        public async Task<bool> IsUserAuthorizedAsync(int projectId, string userId, params ProjectRole[] requiredRoles)
        {
            var project = await _dbContext.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

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
