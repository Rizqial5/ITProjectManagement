using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IProjectMemberRepository
    {
        Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId);
        Task<ProjectMember?> GetMemberAsync(int projectId, string userId);
        Task<bool> AddMemberAsync(int projectId, string userId, ProjectRole role);
        Task<bool> RemoveMemberAsync(int projectId, string userId);
        Task<bool> UpdateMemberRoleAsync(int projectId, string userId, ProjectRole newRole);
        Task<bool> IsUserInProjectAsync(int projectId, string userId);
    }
}
