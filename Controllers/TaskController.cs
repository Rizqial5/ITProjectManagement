using Htmx;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;
using Syncfusion.EJ2.Base;
using System.Security.Claims;
using System.Threading.Tasks;
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

        [Route("task/{projectId:int}/{taskId:int}")]
        public async Task<IActionResult> Details(int projectId, int taskId, bool isConnected)
        {
            var taskItem = await _taskRepository.GetAsync(projectId, taskId);
            var countCommit = isConnected ? await _taskRepository.GetTotalIntegratedCommit(projectId, taskId) : 0;

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
                DueDate = taskItem.TargetDate,
                TotalLinkedCommits = countCommit,
                isConnectedRepo = isConnected,
                Commits = taskItem.Commits.ToList()
                
            };

            if(Request.IsHtmx())
            {
                Response.Htmx(h =>
                {
                    h.PushUrl(Request.GetEncodedUrl());
                });

                return PartialView(taskModel);
            }

            return View(taskModel);
        }


        [HttpGet]
        public IActionResult ShowLinkCommit(int projectId, bool isConnected)
        {
      
            var model = new TaskViewModel
            {
                ProjectId = projectId,
                isConnectedRepo = isConnected
            };


            return PartialView("_LinkCommitGrid", model);

        }

        [HttpGet]
        public async Task<IActionResult> ShowLinkedCommit(int repoId, int taskId)
        {

            var model = await _taskRepository.GetLinkedCommit(repoId, taskId);


            return PartialView("_ListLinkedCommit", model);

        }

        [HttpGet]
        public async Task<IActionResult> EditTaskDetails(int projectId,int taskId)
        {
            var task = await _taskRepository.GetAsync(projectId,taskId); // Pastikan method ini ada di repository Anda
            if (task == null) return NotFound();
            var model = new TaskViewModel
            {
                TaskId = task.Id,
                Description = task.Description,
                Status = task.Status.ToString()
            };
            return PartialView("_EditTaskDetails", model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPanel(int projectId, int taskId)
        {
            var task = await _taskRepository.GetAsync(projectId,taskId);
            if (task == null) return NotFound();
            var model = new TaskViewModel
            {
                TaskId = task.Id,
                Description = task.Description,
                Status = task.Status.ToString()
            };
            return PartialView("_TaskDescPanel", model);
        }


        [HttpPost]
        public async Task<IActionResult> GetCommitRepo([FromBody] DataManagerRequest DataManagerRequest, int projectId, int taskId, bool isConnected)
        {
            IEnumerable<CommitDto> commitData;


            if (isConnected)
            {
                commitData = await _taskRepository.GetAllIntegratedCommitAsync(projectId, taskId);

                
            }
            else
            {
                commitData = new List<CommitDto>(0);
            }

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(commitData, DataManagerRequest);


            return Json(new { result = result, count = commitData.Count()});

        }

        [HttpPost]
        public async Task<IActionResult> GetAvailableCommit([FromBody] DataManagerRequest DataManagerRequest, int projectId, bool isConnected)
        {
            IEnumerable<CommitDto> commitData;


            if (isConnected)
            {
                commitData = await _taskRepository.GetAllCommitAsync(projectId);


            }
            else
            {
                commitData = new List<CommitDto>(0);
            }

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

                TempData["RepoNotification"] = "Commit has been sucessfully linked";

                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                TempData["RepoNotificationFailed"] = "Error when linking commit";
                return Json(new { success = false, message = ex.Message });
            }


        }


        [HttpPost]
        public async Task<IActionResult> SetToDone(int taskId, int projectId)
        {
            try
            {
                var status = Status.Done;

                var result = await _taskRepository.SetTaskStatus(projectId, taskId, status);

                if(result.Success)
                {
                    TempData["RepoNotification"] = result.Message;
                }
                else
                {
                    TempData["RepoNotificationFailed"] = result.Message;
                }

                    return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {

                TempData["RepoNotificationFailed"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }


        }


        [HttpPost]
        public async Task<IActionResult> AddTask(CreateTaskDto newTask, int projectId)
        {
            try
            {
                await _taskRepository.AddAsync(projectId, newTask);

                TempData["RepoNotification"] = $"Task {newTask.Title} added succesfully";

                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                TempData["RepoNotificationFailed"] = ex.Message;

                return Json(new { success = false });
            }

        }

    }
}
