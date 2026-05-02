using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IWorkspaceRepository
    {
        Task<Workspace> CreateDefaultWorkspaceAsync(string userId, string userName);
        Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(string userId);
        Task<bool> IsUserInWorkspaceAsync(int workspaceId, string userId);
        Task<IEnumerable<ApplicationUser>> GetWorkspaceMembersAsync(int workspaceId);
        Task<Workspace?> GetWorkspaceDetailsAsync(int workspaceId);
        Task<bool> UpdateWorkspaceNameAsync(int workspaceId, string newName);
    }
}
