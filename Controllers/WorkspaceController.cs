using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Helpers;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services;
using ProjectManagement.App.Services.Interfaces;
using ProjectManagement.App.ViewModel.Workspace;
using Syncfusion.EJ2.Base;
using System;
using System.Dynamic;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IGithubService _githubService;

        public WorkspaceController(ITaskRepository taskRepository, IProjectRepository projectRepository, IAuthRepository authRepository, IGithubService githubService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authRepository = authRepository;
            _githubService = githubService;
        }


        public async Task<IActionResult> Index(WorkspaceViewModel workspaceViewModel)
        {

            ViewBag.ProjectID = workspaceViewModel.ProjectID;
            ViewBag.ProjectName = workspaceViewModel.ProjectName;

            ViewBag.DllStatus = new List<SelectListItem>
            {
                new("ToDo", "0"),
                new("InProgess", "1"),
                new("Done", "2"),
            };


            // Check if Project has Connected with repo
            var checkConnectProject = await _projectRepository.CheckConnectedProject(workspaceViewModel.ProjectID);

            
            if(checkConnectProject.Success)
            {
                ViewBag.RepoName = checkConnectProject.Data.Name;
                ViewBag.RepoUrl = checkConnectProject.Data.Html_Url;

                //Check and sync latest commit
                

            }

            ViewBag.IsConnected = checkConnectProject.Success;

            // workspaceViewModel.ProjectID dan ProjectName akan terisi dari query string
            return View(workspaceViewModel);
        }

        public IActionResult KanbanView(WorkspaceViewModel workspaceViewModel)
        {

            return PartialView("_KanbanView", workspaceViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> GetTasks([FromBody] DataManagerRequest DataManagerRequest, int projectId)
        {
            var data = await _taskRepository.GetAllAsync(projectId);

            

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(data, DataManagerRequest);

            return Json(new
            {
                result = result,
                count = data.Count()
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] ExpandoObject value, int projectId)
        {
            var task = value.ToTaskItemFromPayload();
            task.ProjectId = projectId;
            await _taskRepository.AddAsync(projectId, task);
            return Json(new {success= true});
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask([FromBody] TaskItem value, int projectId)
        {
            var result = await _taskRepository.UpdateAsync(projectId, value);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask([FromBody] TaskItem value, int projectId)
        {
            var result = await _taskRepository.DeleteAsync(projectId, value.Id);
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult GoToWorkspace(int projectId, string projectName)
        {
            var url = Url.Action("Index", "Workspace", new { ProjectID = projectId, ProjectName = projectName });
            return Json(new { success = true, url });
        }

        [HttpPost]
        public async Task<IActionResult> ConnectToRepo(int projectId, GitHubRepoDto repo)
        {
            repo.Description = repo.Description ?? string.Empty;

            if(!ModelState.IsValid)
            {
                return Json(new ResponseResultDto
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrWhiteSpace(userId))
            {
                return Json(new ResponseResultDto
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var response = await _projectRepository.ConnectRepo(userId, projectId, repo);

            return Json(response);


        }

        [HttpPost]
        public async Task<IActionResult> DisconnectRepo(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var response = await _projectRepository.DisconnectRepo(userId, projectId);

            TempData["RepoNotification"] = "Project has successfully disconnected";

            return Json(new
            {
                Success = response.Success,
                
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCommitRepo(string repoName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


       

            var response = await _authRepository.GetGithubCreds(userId!);
            var githubData = response.Data;

            var creds = await _githubService.CheckLatestCommitAsync(githubData.AccessToken, githubData.GitHubUsername, repoName, "");

            return Json(new { Succes = true });

        }




    }
}

