using ProjectManagement.App.Models;
using ProjectManagement.App.DTO;
using System.Threading.Tasks;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IInviteRepository
    {
        Task<ResponseResultDto<ProjectManagement.App.Models.Workspace.WorkspaceInvite>> CreateInviteAsync(int workspaceId, string inviterUserId, string inviteeEmail);
        Task<ResponseResultDto> AcceptInviteAsync(string token, string inviteeUserId);
        Task<ResponseResultDto> DeclineInviteAsync(string token, string inviteeUserId);
        Task AcceptPendingInvitesForUser(string userEmail, string userId);
    }
}
