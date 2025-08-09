using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;
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
                ProjectOwnerUserId = project.ProjectOwnerUserId
            };

            await _dbContext.Projects.AddAsync(newProject);
            await _dbContext.SaveChangesAsync();
        }

 
        public async Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId)
        {
            var existData = await _dbContext.GithubRepoConnecteds
                .Include(c=> c.Repo).FirstOrDefaultAsync(i => i.ProjectId == projectId);

            if (existData == null)
            {
                return new()
                {
                    Success = false,
                    Message = "Data is Not exists"
                };
            }

            var repoDto = new GitHubRepoDto
            {
                Name = existData.Repo.RepoName,
                Html_Url = existData.Repo.RepoUrl
               
            };

            return new()
            {
                Success = true,
                Data = repoDto
                
            };
        }
        

        public async Task<ResponseResultDto> ConnectRepo(string userId, int projectId, GitHubRepoDto githubRepoDto)
        {

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // first create repo data

                var newRepo = new GithubRepo
                {
                    RepoId = githubRepoDto.RepoId,
                    RepoName = githubRepoDto.Name,
                    RepoUrl = githubRepoDto.Html_Url,
                };

                await _dbContext.GithubRepos.AddAsync(newRepo);
                await _dbContext.SaveChangesAsync();

                var newGithubConnected = new GithubRepoConnected
                {
                    ProjectId = projectId,
                    RepoId = newRepo.RepoId,
                    UserId = userId
                };

                await _dbContext.GithubRepoConnecteds.AddAsync(newGithubConnected);

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

                return new()
                {
                    Success = false,
                    Message = ex.Message
                };

            }


        }

        public async Task<bool> DeleteAsync(int[] id, string userId)
        {
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

        public async Task<IEnumerable<Project>> GetAllAsync(string userId)
        {
            return await _dbContext.Projects
                .Where(p => p.ProjectOwnerUserId == userId)
                .ToListAsync();
        }

        public async Task<Project?> GetAsync(int id, string userId)
        {
            return await _dbContext.Projects
                .FirstOrDefaultAsync(i => i.Id == id && i.ProjectOwnerUserId == userId);
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            if (string.IsNullOrEmpty(project.ProjectOwnerUserId))
                return false;

            var existing = await _dbContext.Projects
                .FirstOrDefaultAsync(i => i.Id == project.Id && i.ProjectOwnerUserId == project.ProjectOwnerUserId);

            if (existing == null)
                return false;

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.CreatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
