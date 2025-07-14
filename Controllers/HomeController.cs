using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;

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

            var existingProjects =  await _projectRepository.GetAllAsync();

            ViewBag.IsProjectEmpty = false;

            if (!existingProjects.Any())
            {
                ViewBag.IsProjectEmpty = true;
            }

            return View();
        }

        public async Task<IActionResult> CreateProject(CreateProjectDto createProjectDto)
        {
            if (!ModelState.IsValid)
            {

                return Json(new { success = false });
            }


            await _projectRepository.AddAsync(createProjectDto);

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
