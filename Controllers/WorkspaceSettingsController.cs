using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Repository.Interface;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    [Authorize]
    public class WorkspaceSettingsController : Controller
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IAuthRepository _authRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public WorkspaceSettingsController(
            IWorkspaceRepository workspaceRepository,
            IAuthRepository authRepository,
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _workspaceRepository = workspaceRepository;
            _authRepository = authRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null || userId == null) return RedirectToAction("Index", "Home");

            var workspace = await _workspaceRepository.GetWorkspaceDetailsAsync(workspaceId.Value);
            if (workspace == null) return NotFound();

            var model = new WorkspaceSettingsDto
            {
                Id = workspace.Id,
                Name = workspace.Name,
                OwnerName = workspace.Owner?.UserName ?? "Unknown",
                CreatedAt = workspace.CreatedAt,
                Members = workspace.Members.Select(m => new WorkspaceMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.User?.UserName ?? "Unknown",
                    Email = m.User?.Email ?? "Unknown",
                    Role = m.Role.ToString(),
                    JoinedAt = m.JoinedAt
                }).ToList()
            };

            // Populate joined workspaces
            var joinedWorkspaces = await _workspaceRepository.GetUserWorkspaceDetailsAsync(userId);
            model.UserWorkspaces = joinedWorkspaces.Select(jw => new UserWorkspaceDto
            {
                WorkspaceId = jw.WorkspaceId,
                WorkspaceName = jw.Workspace?.Name ?? "Unknown",
                Role = jw.Role.ToString(),
                JoinedAt = jw.JoinedAt
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(WorkspaceSettingsDto model)
        {
            var workspaceId = User.GetWorkspaceId();
            if (workspaceId == null || workspaceId != model.Id) return Json(new { success = false, message = "Unauthorized." });

            if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid data." });

            var success = await _workspaceRepository.UpdateWorkspaceNameAsync(workspaceId.Value, model.Name);
            
            if (success)
            {
                TempData["RepoNotification"] = "Workspace name updated successfully. Please re-login to see the change in the header.";
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to update workspace name." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberDto model)
        {
            var workspaceId = User.GetWorkspaceId();
            var inviterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inviterName = User.Identity?.Name ?? "Someone";

            if (workspaceId == null || inviterId == null)
            {
                return Json(new { success = false, message = "User context is missing." });
            }

            if (!ModelState.IsValid)
            {
                 var errors = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Json(new { success = false, message = "Validation failed: " + errors });
            }

            var userToInvite = await _userRepository.GetUserByEmailAsync(model.Email);
            if (userToInvite == null)
            {
                return Json(new { success = false, message = $"User with email '{model.Email}' not found." });
            }

            var isAlreadyMember = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId.Value, userToInvite.Id);
            if (isAlreadyMember)
            {
                return Json(new { success = false, message = "This user is already a member of your workspace." });
            }

            // In a real app, this would use IInviteRepository to create a pending invite
            // For now, as part of SaaS phase 2, let's assume direct invite + notification
            var workspace = await _workspaceRepository.GetWorkspaceDetailsAsync(workspaceId.Value);

            await _notificationRepository.CreateNotificationAsync(
                recipientUserId: userToInvite.Id,
                title: "Workspace Invitation",
                message: $"{inviterName} invited you to join the '{workspace?.Name}' workspace.",
                url: Url.Action("Index", "WorkspaceSettings"),
                iconCssClass: "fas fa-user-plus"
            );

            return Json(new { success = true, message = $"An invitation notification has been sent to {model.Email}." });
        }
    }
}
