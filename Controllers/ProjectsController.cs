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

        public async Task<IActionResult> Page()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var allProject = await _projectRepository.GetAllAsync(userId);

            var data = allProject.Select(i => new ProjectCardViewModel()
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



            return View(data);
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
