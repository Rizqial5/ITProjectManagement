using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
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

        public async Task<IActionResult> Index()
        {
            //Check if there is project 
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrWhiteSpace(userId))
            {
                return View(new List<Project>());
            }

            var existingProjects =  await _projectRepository.GetAllAsync(userId);

            ViewBag.IsProjectEmpty = false;
            ViewBag.IsUserLogin = User.Identity?.IsAuthenticated;
            ViewBag.UserId = userId;


            if (!existingProjects.Any())
            {
                ViewBag.IsProjectEmpty = true;
            }

            return View(existingProjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectDto createProjectDto)
        {
            if (!ModelState.IsValid)
            {

                return Json(new { success = false });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            createProjectDto.ProjectOwnerUserId = userId;

            await _projectRepository.AddAsync(createProjectDto);

            return Json(new { success = true });
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

            return Json(new { success = true, data = refreshData });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject([FromBody] CRUDModel<Project> value)
        {
            var result = await _projectRepository.UpdateAsync(value.value);

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
