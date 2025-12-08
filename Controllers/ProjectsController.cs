using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Interfaces;
using ProjectManagement.App.ViewModel.Project;
using ProjectManagement.App.ViewModel.Workspace;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectRepository _projectRepository; 
        private readonly IGithubService _githubService;
        private readonly IGithubRepository _githubRepo;

        public ProjectsController(IProjectRepository projectRepository, IGithubService githubService, IGithubRepository githubRepo)
        {
            _projectRepository = projectRepository;
            _githubService = githubService;
            _githubRepo = githubRepo;
        }

        public async Task<IActionResult> Page(int page = 1, int pageSize = 12)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var allProject = await _projectRepository.GetAllAsync(userId);

            var totalRecords = allProject.Count();

            var data = allProject
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(i=> i.CreatedAt)
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
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Status = project.Status.ToString(),
                DueDate = project.DueDate,
                IsConnected = project.IsConnected,
                Progress = project.Progress,
                TotalTasks = project.TotalTasks,
                CompletedTasks = project.CompletedTasks,
                TotalCommits = projectData.Tasks.Sum(i => i.Commits.Count()),
                Members = new string[] { "Test", "Agus" },
                Tasks = projectData.Tasks.Any() ? projectData.Tasks.Select(i => new ProjectTaskDto()
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Commits = i.Commits.Count(),
                    DueDate = i.TargetDate,
                    Status = i.Status.ToString()
                }).OrderByDescending(i => i.DueDate).ToArray() : Array.Empty<ProjectTaskDto>()

            };

            var checkConnectProject = await _projectRepository.CheckConnectedProject(project.Id);

            if (checkConnectProject.Success)
            {
                await SynchronizeCommitAsync(checkConnectProject);
            }

            return View(data);
        }

        private async Task SynchronizeCommitAsync(ResponseResultDto<GitHubRepoDto> checkConnectProject)
        {
            var repoData = checkConnectProject.Data;


            if (!User.IsConnectedGithub())
            {
                var errMessage = "User github credential is expired or not found";

                throw new Exception(errMessage);
            }

            var accesToken = User.GetGithubToken();

            repoData.RepoOwner = User.GetGithubUsername();

            var commitResponse = await _githubService.GetCommitsAsync(repoData, accesToken);

            if (commitResponse.Success)
            {
                var commitData = commitResponse.Data;

                var existingShas = repoData.Commits.Select(c => c.Sha).ToHashSet();

                var newCommits = commitData.Where(c => !existingShas.Contains(c.Sha)).ToList();

                if (newCommits.Any())
                {
                    var repoResponse = await _githubRepo.InsertGithubCommit(newCommits, repoData.RepoId);

                    if (repoResponse.Success)
                    {
                        TempData["RepoNotification"] = "Commits has been succesfully Synchronize";
                    }
                    else
                    {
                        TempData["RepoNotification"] = "Failed to synchronize commit data";
                    }
                }
            }
        }
    }
}
