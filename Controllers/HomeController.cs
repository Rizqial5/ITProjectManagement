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

        public HomeController(ILogger<HomeController> logger, IProjectRepository projectRepository)
        {
            _logger = logger;
            _projectRepository = projectRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //Check if there is project 
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var accessToken = HttpContext.Session.GetString("GitHubToken");
            var githubUsername = HttpContext.Session.GetString("GitHubUser");

            ViewBag.IsProjectEmpty = false;
            ViewBag.IsUserLogin = User.Identity?.IsAuthenticated;
            ViewBag.UserId = userId;
            ViewBag.Github = githubUsername ?? string.Empty;



            if (string.IsNullOrWhiteSpace(userId))
            {
                return View(new List<ProjectViewModel>());
            }


            var existingProjects =  await _projectRepository.GetAllAsync(userId);








            var showData = existingProjects.Select(i => new ProjectViewModel
            {
                Id = i.Id,
                Title = i.Name,
                Description = i.Description ?? string.Empty,
                Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
                TaskTotal = i.Tasks.Count,
                TaskComplete = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0
            }).Take(3);

            DashboardViewModel dashboardData = new()
            {
                TotalProjects = showData.Count(),
                RecentProjects = showData,
                TotalTasks = showData.Sum(i=> i.TaskTotal),
                TotalCompletedTasks = showData.Sum(i=> i.TaskComplete)
            }; 


            if (!showData.Any())
            {
                ViewBag.IsProjectEmpty = true;
            }

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

            var result = await _projectRepository.UpdateAsync(updatedProject);

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
