using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Project;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectsController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IActionResult> Page(int page = 1, int pageSize = 12)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var allProject = await _projectRepository.GetAllAsync(userId);

            var totalRecords = allProject.Count();

            var data = allProject
                .Skip((page-1) * pageSize)
                .Take(pageSize)
                .Select(i => new ProjectCardViewModel()
                {
                    Id = i.Id,
                    Title = i.Name,
                    Description = i.Description,
                    Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
                    TotalTasks = i.Tasks.Count,
                    CompletedTasks = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0,
                    DueDate = i.EndDate,
                    IsConnected = i.GithubRepoConnecteds.Any(i => i.Connected)
                });

            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;


            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetPagedProjects(int currentPage, int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var allProject = await _projectRepository.GetAllAsync(userId);


            var data = allProject
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new ProjectCardViewModel()
                {
                    Id = i.Id,
                    Title = i.Name,
                    Description = i.Description,
                    Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
                    TotalTasks = i.Tasks.Count,
                    CompletedTasks = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0,
                    DueDate = i.EndDate,
                    IsConnected = i.GithubRepoConnecteds.Any(i => i.Connected)
                }).ToList();

            return PartialView("_ProjectCardListPartial", data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //var allProject = await _projectRepository.GetAllAsync(userId);

            //var data = allProject.Select(i => new ProjectCardViewModel()
            //{
            //    Id = i.Id,
            //    Title = i.Name,
            //    Description = i.Description,
            //    Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
            //    TotalTasks = i.Tasks.Count,
            //    CompletedTasks = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0,
            //    DueDate = DateTime.MaxValue,
            //    IsConnected = false
            //});



            return View();
        }
    }
}
