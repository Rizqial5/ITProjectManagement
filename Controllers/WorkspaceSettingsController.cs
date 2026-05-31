using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Models;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public WorkspaceSettingsController(
            IWorkspaceRepository workspaceRepository,
            IAuthRepository authRepository,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _workspaceRepository = workspaceRepository;
            _authRepository = authRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
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
        public async Task<IActionResult> SwitchWorkspace(int workspaceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var isMember = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
            if (!isMember)
            {
                return Json(new { success = false, message = "You do not have access to this workspace." });
            }

            HttpContext.Session.SetInt32("ActiveWorkspaceId", workspaceId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["RepoNotification"] = "Workspace switched successfully.";

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
    }
}
