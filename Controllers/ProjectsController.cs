using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Project;
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
                .Select(ConvertToProjectViewModel);

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
                .Select(ConvertToProjectViewModel).ToList();

            return PartialView("_ProjectCardListPartial", data);
        }

        private static ProjectCardViewModel ConvertToProjectViewModel(Models.Workspace.Project i)
        {
            return new ProjectCardViewModel()
            {
                Id = i.Id,
                Title = i.Name,
                Description = i.Description,
                Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
                TotalTasks = i.Tasks.Count,
                CompletedTasks = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0,
                DueDate = i.EndDate,
                IsConnected = i.GithubRepoConnecteds.Any(i => i.Connected)
            };
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var projectData = await _projectRepository.GetAsync(id,userId);

            var project = ConvertToProjectViewModel(projectData);


            var data = new ProjectDetailsDto()
            {
                Title = project.Title,
                Description = project.Description,
                Status = project.Status.ToString(),
                DueDate = project.DueDate,
                IsConnected = true,
                Progress = project.Progress,
                TotalTasks = project.TotalTasks,
                CompletedTasks = project.CompletedTasks,
                TotalCommits = 0,
                Members = new string[] { "Test", "Agus" },
                Tasks = new ProjectTaskDto[] {
                    new() { Title = "Create project documentation", Description = "Write comprehensive API documentation", Assignee = "Mike Johnson", DueDate = new DateTime(2024,3,15), Priority = "Low", Status = "To Do", Commits = 0 },
                    new() { Title = "Database optimization", Description = "Optimize queries and add indexing", Assignee = "David Lee", DueDate = new DateTime(2024,3,25), Priority = "High", Status = "In Progress", Commits = 5 }
                }
            };



            return View(data);
        }
    }
}
