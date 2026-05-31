using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO.Search;
using ProjectManagement.App.Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.App.Repository
{
    public class SearchRepository : ISearchRepository
    {
        private readonly AppDbContext _dbContext;

        public SearchRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SearchQueryResultDto> SearchWorkspaceAsync(string query, int workspaceId)
        {
            var result = new SearchQueryResultDto { Query = query };

            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            var cleanQuery = query.Trim().ToLower();

            // 1. Projects
            result.Projects = await _dbContext.Projects
                .Where(p => p.WorkspaceId == workspaceId && 
                            (p.Name.ToLower().Contains(cleanQuery) || 
                             (p.Description != null && p.Description.ToLower().Contains(cleanQuery))))
                .Select(p => new SearchProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .Take(5) // Limit to top 5 results for dropdown
                .ToListAsync();

            // 2. Tasks
            result.Tasks = await _dbContext.TaskItems
                .Include(t => t.Project)
                .Where(t => t.Project.WorkspaceId == workspaceId && 
                            (t.Title.ToLower().Contains(cleanQuery) || 
                             (t.Description != null && t.Description.ToLower().Contains(cleanQuery))))
                .Select(t => new SearchTaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    ProjectName = t.Project.Name,
                    Status = t.Status.ToString()
                })
                .Take(5) // Limit to top 5 results for dropdown
                .ToListAsync();

            // 3. Workspace Members
            result.Members = await _dbContext.WorkspaceMembers
                .Include(wm => wm.User)
                .Where(wm => wm.WorkspaceId == workspaceId && 
                            (wm.User.UserName != null && wm.User.UserName.ToLower().Contains(cleanQuery) || 
                             wm.User.Email != null && wm.User.Email.ToLower().Contains(cleanQuery)))
                .Select(wm => new SearchMemberDto
                {
                    UserId = wm.UserId,
                    UserName = wm.User.UserName ?? string.Empty,
                    Email = wm.User.Email ?? string.Empty,
                    Role = wm.Role.ToString()
                })
                .Take(5) // Limit to top 5 results for dropdown
                .ToListAsync();

            return result;
        }
    }
}
