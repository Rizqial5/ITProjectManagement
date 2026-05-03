using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Repository.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagement.App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class InvitesController : Controller
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserRepository _userRepository;

        public InvitesController(
            IInviteRepository inviteRepository,
            INotificationRepository notificationRepository,
            IWorkspaceRepository workspaceRepository,
            IUserRepository userRepository)
        {
            _inviteRepository = inviteRepository;
            _notificationRepository = notificationRepository;
            _workspaceRepository = workspaceRepository;
            _userRepository = userRepository;
        }

        [HttpPost("send-workspace-invite")]
        public async Task<IActionResult> SendWorkspaceInvite([FromBody] InviteMemberDto model)
        {
            var workspaceId = User.GetWorkspaceId();
            var inviterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inviterName = User.Identity?.Name ?? "Someone";

            if (workspaceId == null || inviterId == null)
            {
                return BadRequest(new { success = false, message = "User context is missing." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid email format." });
            }

            var result = await _inviteRepository.CreateInviteAsync(workspaceId.Value, inviterId, model.Email);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // If a user with that email already exists, send them an in-app notification.
            var invite = result.Data as ProjectManagement.App.Models.Workspace.WorkspaceInvite;
            var userToInvite = await _userRepository.GetUserByEmailAsync(model.Email);

            if (userToInvite != null && invite != null)
            {
                var workspace = await _workspaceRepository.GetWorkspaceDetailsAsync(workspaceId.Value);
                await _notificationRepository.CreateNotificationAsync(
                    recipientUserId: userToInvite.Id,
                    title: "Workspace Invitation",
                    message: $"{inviterName} invited you to join '{workspace?.Name}'.",
                    url: Url.Action("Accept", "Invites", new { token = invite.Token }, Request.Scheme),
                    declineUrl: Url.Action("Decline", "Invites", new { token = invite.Token }, Request.Scheme),
                    iconCssClass: "fas fa-user-plus",
                    relatedInviteId: invite.Id,
                    type: ProjectManagement.App.Models.NotificationType.WorkspaceInvite
                );
            }
            // If user doesn't exist, they will get an email link (to be implemented later)

            return Ok(new { success = true, message = $"An invitation has been sent to {model.Email}." });
        }

        [HttpGet("accept/{token}")]
        public async Task<IActionResult> Accept(string token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Must be logged in
            }

            var result = await _inviteRepository.AcceptInviteAsync(token, userId);

            if (result.Success)
            {
                TempData["RepoNotification"] = result.Message;
            }
            else
            {
                TempData["RepoNotificationFailed"] = result.Message;
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("decline/{token}")]
        public async Task<IActionResult> Decline(string token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Must be logged in
            }

            var result = await _inviteRepository.DeclineInviteAsync(token, userId);
            
            TempData["RepoNotification"] = result.Message;
            
            return RedirectToAction("Index", "Home");
        }
    }
}
