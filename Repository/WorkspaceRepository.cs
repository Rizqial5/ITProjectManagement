using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;

namespace ProjectManagement.App.Repository
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly AppDbContext _dbContext;

        public WorkspaceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Workspace> CreateDefaultWorkspaceAsync(string userId, string userName)
        {
            var workspace = new Workspace
            {
                Name = $"{userName}'s Workspace",
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Workspaces.AddAsync(workspace);
            await _dbContext.SaveChangesAsync();

            var workspaceMember = new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = userId,
                Role = WorkspaceRole.Admin,
                JoinedAt = DateTime.UtcNow
            };

            await _dbContext.WorkspaceMembers.AddAsync(workspaceMember);
            await _dbContext.SaveChangesAsync();

            return workspace;
        }

        public async Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(string userId)
        {
            return await _dbContext.WorkspaceMembers
                .Where(wm => wm.UserId == userId)
                .Include(wm => wm.Workspace)
                .Select(wm => wm.Workspace)
                .ToListAsync();
        }

        public async Task<bool> IsUserInWorkspaceAsync(int workspaceId, string userId)
        {
            return await _dbContext.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetWorkspaceMembersAsync(int workspaceId)
        {
            return await _dbContext.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId)
                .Include(wm => wm.User)
                .Select(wm => wm.User)
                .ToListAsync();
        }

        public async Task<Workspace?> GetWorkspaceDetailsAsync(int workspaceId)
        {
            return await _dbContext.Workspaces
                .Include(w => w.Owner)
                .Include(w => w.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
        }

        public async Task<bool> UpdateWorkspaceNameAsync(int workspaceId, string newName)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null) return false;

            workspace.Name = newName;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
