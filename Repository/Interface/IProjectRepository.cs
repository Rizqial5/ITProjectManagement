using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync(string userId);

        Task<Project> GetAsync(int id, string userId);

        Task AddAsync(CreateProjectDto project);
        Task<bool> UpdateAsync(Project project);

        Task<bool> DeleteAsync(int[] id, string userId);

        Task<ResponseResultDto> ConnectRepo(string userId, int projectId, GitHubRepoDto githubRepoDto);
        Task<ResponseResultDto<GitHubRepoDto>> CheckConnectedProject(int projectId);

    }
}
