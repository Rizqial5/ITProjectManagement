using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync(string userId);

        Task<Project?> GetAsync(int id, string userId);

        Task AddAsync(CreateProjectDto project);
        Task<bool> UpdateAsync(Project project, string userId);

        Task<bool> DeleteAsync(int[] id, string userId);

        Task<ResponseResultDto> ConnectRepo(string userId, int projectId, GitHubRepoDto githubRepoDto);
        Task<ResponseResultDto> DisconnectRepo(string userId, int projectId);
        Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId, string userId);

        Task<IEnumerable<Project>> GetActiveProjectsAsync(string userId, int take);
        Task<(int TotalProjects, int TotalTasks, int TotalCompletedTasks)> GetDashboardStatsAsync(string userId);

        Task<bool> IsUserAuthorizedAsync(int projectId, string userId, params ProjectRole[] requiredRoles);
    }
}
