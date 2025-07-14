using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO.Project;
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

            var newProject = new Project
            {
                Name = project.Title,
                Description = project.Description,
                CreatedAt = DateTime.UtcNow,
            };

            await _dbContext.Projects.AddAsync(newProject);

            await _dbContext.SaveChangesAsync();
            
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            var results = await _dbContext.Projects.ToListAsync();

            return results;
        }

        public Task<Project> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
