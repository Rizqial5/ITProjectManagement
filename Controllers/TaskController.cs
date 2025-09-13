using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;
using Syncfusion.EJ2.Base;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectManagement.App.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepository;

        public TaskController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpGet("/project/{projectId}/task/{taskId}")]
        public async Task<IActionResult> Details(int projectId, int taskId)
        {
            var taskItem = await _taskRepository.GetAsync(projectId, taskId);
            var countCommit = await _taskRepository.GetTotalIntegratedCommit(projectId);

            if(taskItem == null)
            {
                TempData["RepoNotificationFailed"] = "Task is not exists";
                return RedirectToAction("Index", "Workspace", new { ProjectID = projectId });
            }

            var taskModel = new TaskViewModel()
            {
                TaskId = taskItem.Id,
                Title = taskItem.Title,
                ProjectName = taskItem.Project.Name,
                ProjectId = taskItem.Project.Id,
                Description = taskItem.Description,
                Status = taskItem.Status.ToString(),
                TotalLinkedCommits = countCommit
                
            };

            return View(taskModel);
        }


        [HttpPost]
        public async Task<IActionResult> GetCommitRepo([FromBody] DataManagerRequest DataManagerRequest, int projectId)
        {
            var commitData = await _taskRepository.GetAllIntegratedCommitAsync(projectId);

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(commitData, DataManagerRequest);

            return Json(new { result = result, count = commitData.Count()});

        }

        [HttpPost]
        public async Task<IActionResult> GetAvailableCommit([FromBody] DataManagerRequest DataManagerRequest, int projectId)
        {
            var commitData = await _taskRepository.GetAllCommitAsync(projectId);

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(commitData, DataManagerRequest);

            return Json(new { result = result, count = commitData.Count() });

        }

        [HttpPost]
        public async Task<IActionResult> LinkCommit(int commitId, int repoId, int taskId)
        {
            try
            {
                var result = await _taskRepository.ConnectCommitToTaskAsync(repoId, commitId, taskId);


                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }


        }

    }
}
