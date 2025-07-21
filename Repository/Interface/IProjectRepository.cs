using ProjectManagement.App.DTO.Project;
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

    }
}
