using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Project;
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

        public HomeController(ILogger<HomeController> logger, IProjectRepository projectRepository, IGithubRepository githubRepository)
        {
            _logger = logger;
            _projectRepository = projectRepository;
            _githubRepository = githubRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return View(new DashboardViewModel());
            }

            var githubUsername = HttpContext.Session.GetString("GitHubUser");
            ViewBag.Github = githubUsername ?? string.Empty;

            // Get dashboard stats through repository
            var stats = await _projectRepository.GetDashboardStatsAsync(userId);

            // Get Active Projects through repository
            var activeProjectsData = await _projectRepository.GetActiveProjectsAsync(userId, 4);

            var recentProjects = activeProjectsData
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Name,
                    Description = p.Description ?? string.Empty,
                    Status = p.Tasks != null && p.Tasks.Any(t => t.Status == Models.Enum.Status.InProgress) ? Models.Enum.Status.InProgress : Models.Enum.Status.ToDo,
                    TaskTotal = p.Tasks?.Count ?? 0,
                    TaskComplete = p.Tasks?.Count(t => t.Status == Models.Enum.Status.Done) ?? 0
                }).ToList();

            // Get Recent Activity through repository
            var recentCommits = await _githubRepository.GetRecentCommitsAsync(userId, 4);
            
            var recentActivities = recentCommits
                .Select(c => new RecentActivityViewModel
                {
                    Title = c.Message ?? "New commit",
                    Author = c.AuthorName,
                    ProjectName = c.Task?.Project?.Name ?? "Unknown Project",
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

            ViewBag.IsProjectEmpty = stats.TotalProjects == 0;
            ViewBag.HideNavAndSidebar = stats.TotalProjects == 0;

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

                    return Json(new { success = false });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                createProjectDto.ProjectOwnerUserId = userId;

                await _projectRepository.AddAsync(createProjectDto);

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

            var result = await _projectRepository.DeleteAsync(projectId, userId);

            if(!result)
            {
                return Json(new { success = false, message = "No Project found to delete" });
            }

            var refreshData = await _projectRepository.GetAllAsync(userId);

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

            var updatedProject = new Project
            {
                Id = value.value.Id,
                CreatedAt = DateTime.UtcNow,
                Description = value.value.Description,
                Name = value.value.Title,
                ProjectOwnerUserId = userId,
            };

            var result = await _projectRepository.UpdateAsync(updatedProject, userId!);

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
