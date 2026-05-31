using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Home;
using System.Diagnostics;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectRepository _projectRepository;
        private readonly IGithubRepository _githubRepository;
        private readonly IWorkspaceRepository _workspaceRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IProjectRepository projectRepository,
            IGithubRepository githubRepository,
            IWorkspaceRepository workspaceRepository)
        {
            _logger = logger;
            _projectRepository = projectRepository;
            _githubRepository = githubRepository;
            _workspaceRepository = workspaceRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (string.IsNullOrWhiteSpace(userId) || workspaceId == null)
            {
                return View(new DashboardViewModel());
            }

            var githubUsername = HttpContext.Session.GetString("GitHubUser");
            ViewBag.Github = githubUsername ?? string.Empty;

            // Get dashboard stats through repository
            var stats = await _projectRepository.GetDashboardStatsAsync(workspaceId.Value, userId);

            // Get Active Projects through repository
            var activeProjectsData = await _projectRepository.GetActiveProjectsAsync(workspaceId.Value, userId, 4);

            var recentProjects = activeProjectsData
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Name,
                    Description = p.Description ?? string.Empty,
                    Status = p.Tasks != null && p.Tasks.Any(t => t.Status == Models.Enum.Status.InProgress) ? Models.Enum.Status.InProgress : Models.Enum.Status.ToDo,
                    TaskTotal = p.Tasks?.Count ?? 0,
                    TaskComplete = p.Tasks?.Count(t => t.Status == Models.Enum.Status.Done) ?? 0,
                    MemberInitials = p.ProjectMembers
                        .Where(m => m.IsActive && m.User != null)
                        .Select(m => {
                            var name = m.User.UserName ?? m.User.Email ?? "?";
                            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            var initials = parts.Length > 1 
                                ? string.Concat(parts[0][0], parts[1][0]) 
                                : parts[0].Length > 1 ? parts[0].Substring(0, 2) : parts[0];
                            return initials.ToUpper();
                        }).ToList()
                }).ToList();

            // Get Recent Activity through repository
            var recentCommits = await _githubRepository.GetRecentCommitsAsync(userId, 4);

            var recentActivities = recentCommits
                .Select(c => new RecentActivityViewModel
                {
                    Title = c.Message ?? "New commit",
                    Author = c.AuthorName,
                    ProjectName = c.ProjectName ?? "Unknown Project",
                    Date = c.CommitDate,
                    ActivityType = "Commit",
                    Status = c.Task?.Status
                })
                .ToList();

            DashboardViewModel dashboardData = new()
            {
                TotalProjects = stats.TotalProjects,
                TotalTasks = stats.TotalTasks,
                TotalCompletedTasks = stats.TotalCompletedTasks,
                RecentProjects = recentProjects,
                RecentActivities = recentActivities
            };

            var workspaces = await _workspaceRepository.GetUserWorkspacesAsync(userId);
            var hasMultipleWorkspaces = workspaces.Count() > 1;
            var hasSwitched = HttpContext.Session.GetInt32("ActiveWorkspaceId").HasValue;

            ViewBag.IsProjectEmpty = stats.TotalProjects == 0;
            ViewBag.ShowLandingPage = stats.TotalProjects == 0 && !hasSwitched;
            ViewBag.HideNavAndSidebar = stats.TotalProjects == 0 && !hasMultipleWorkspaces;

            return View(dashboardData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectDto createProjectDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));
                    return Json(new { success = false, message = "Validation failed: " + errors });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var workspaceId = User.GetWorkspaceId();

                if (workspaceId == null) return Json(new { success = false, message = "Workspace not found." });

                createProjectDto.ProjectOwnerUserId = userId;

                await _projectRepository.AddAsync(createProjectDto, workspaceId.Value);

                TempData["RepoNotification"] = "Project successfully created";

                 return Json(new { success = true });
            }
            catch(Exception ex)
            {
                TempData["RepoNotificationFailed"] = "Error when create project " + ex.Message;

                return Json(new { success = false });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject([FromBody] int[] projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return Json(new { success = false, message = "Workspace not found." });

            var result = await _projectRepository.DeleteAsync(projectId, workspaceId.Value, userId);

            if(!result)
            {
                return Json(new { success = false, message = "No Project found to delete" });
            }

            var refreshData = await _projectRepository.GetAllAsync(workspaceId.Value, userId);

            var showData = refreshData.Select(i => new ProjectViewModel
            {
                Id = i.Id,
                Title = i.Name,
                Description = i.Description ?? string.Empty
            });

            return Json(new { success = true, data = showData });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject([FromBody] CRUDModel<ProjectViewModel> value)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return Json(new { success = false, message = "Workspace not found." });

            var updatedProject = new Project
            {
                Id = value.value.Id,
                CreatedAt = DateTime.UtcNow,
                Description = value.value.Description,
                Name = value.value.Title,
                ProjectOwnerUserId = userId,
            };

            var result = await _projectRepository.UpdateAsync(updatedProject, workspaceId.Value, userId!);

            if (!result)
            {
                return Json(new { success = false, message = "No Project found to update" });
            }

            return Json(new { success = true });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
