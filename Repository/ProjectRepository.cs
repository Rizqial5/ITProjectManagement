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

        public async Task<bool> DeleteAsync(int[] id)
        {
            var projectSelected = await _dbContext.Projects.Where(i => id.Contains(i.Id)).ToListAsync();

            if (projectSelected.Any()) 
            {
                _dbContext.Projects.RemoveRange(projectSelected);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            return false;
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

        public async Task<bool> UpdateAsync(Project project)
        {
            var checkData = await _dbContext.Projects.FirstOrDefaultAsync(i => i.Id == project.Id);

            if(checkData == null)
            {
                return false;
            }

            checkData.Name = project.Name;
            checkData.Description = project.Description;
            checkData.CreatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
