using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync(int workspaceId, string userId);

        Task<Project?> GetAsync(int id, int workspaceId, string userId);

        Task AddAsync(CreateProjectDto project, int workspaceId);
        Task<bool> UpdateAsync(Project project, int workspaceId, string userId);

        Task<bool> DeleteAsync(int[] id, int workspaceId, string userId);

        Task<ResponseResultDto> ConnectRepo(string userId, int projectId, int workspaceId, GitHubRepoDto githubRepoDto);
        Task<ResponseResultDto> DisconnectRepo(string userId, int projectId, int workspaceId);
        Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId, int workspaceId, string userId);

        Task<IEnumerable<Project>> GetActiveProjectsAsync(int workspaceId, string userId, int take);
        Task<(int TotalProjects, int TotalTasks, int TotalCompletedTasks)> GetDashboardStatsAsync(int workspaceId, string userId);

        Task<bool> IsUserAuthorizedAsync(int projectId, int workspaceId, string userId, params ProjectRole[] requiredRoles);
    }
}
