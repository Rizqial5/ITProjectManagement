using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.Models;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();

        Task<Project> GetAsync(int id);

        Task AddAsync(CreateProjectDto project);
        Task UpdateAsync(Project project);

        Task DeleteAsync(int id);

    }
}
