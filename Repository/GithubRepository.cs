using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Repository.Interface;

namespace ProjectManagement.App.Repository
{
    public class GithubRepository : IGithubRepository
    {
        private readonly AppDbContext _dbContext;

        public GithubRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
