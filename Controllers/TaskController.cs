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
                Status = taskItem.Status.ToString()

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

    }
}
