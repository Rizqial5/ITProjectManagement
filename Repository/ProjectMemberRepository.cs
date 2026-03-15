using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;

namespace ProjectManagement.App.Repository
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectMemberRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId)
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId && pm.IsActive)
                .OrderBy(pm => pm.Role)
                .ToListAsync();
        }

        public async Task<ProjectMember?> GetMemberAsync(int projectId, string userId)
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task<bool> IsUserInProjectAsync(int projectId, string userId)
        {
            return await _dbContext.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.IsActive);
        }

        public async Task<bool> AddMemberAsync(int projectId, string userId, ProjectRole role)
        {
            // Cek apakah sudah terdaftar
            var existing = await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (existing != null)
            {
                if (existing.IsActive) return false; // Sudah ada dan aktif
                
                // Re-aktifkan jika sebelumnya pernah dihapus (soft delete logic)
                existing.IsActive = true;
                existing.Role = role;
                existing.JoinedAt = DateTime.UtcNow;
            }
            else
            {
                var member = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _dbContext.ProjectMembers.AddAsync(member);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int projectId, string userId)
        {
            var member = await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null) return false;

            // Jangan biarkan Owner dihapus lewat sini (bisnis rule)
            if (member.Role == ProjectRole.Owner) return false;

            _dbContext.ProjectMembers.Remove(member);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberRoleAsync(int projectId, string userId, ProjectRole newRole)
        {
            var member = await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null) return false;

            member.Role = newRole;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
