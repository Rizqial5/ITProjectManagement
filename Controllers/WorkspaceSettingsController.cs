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

        public WorkspaceSettingsController(IWorkspaceRepository workspaceRepository)
        {
            _workspaceRepository = workspaceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var workspaceId = User.GetWorkspaceId();
            if (workspaceId == null) return RedirectToAction("Index", "Home");

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
                // Note: The User Claim for WorkspaceName will be outdated until the next login.
                // In a real app, we might want to refresh the cookie/claims here.
                TempData["RepoNotification"] = "Workspace name updated successfully. Please re-login to see the change in the header.";
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to update workspace name." });
        }
    }
}
