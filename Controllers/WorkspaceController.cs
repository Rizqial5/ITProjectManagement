using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagement.App.Helpers;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Workspace;
using Syncfusion.EJ2.Base;
using System.Dynamic;

namespace ProjectManagement.App.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly ITaskRepository _taskRepository;

        public WorkspaceController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
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

        public IActionResult Index(WorkspaceViewModel workspaceViewModel)
        {

            ViewBag.ProjectID = workspaceViewModel.ProjectID;
            ViewBag.ProjectName = workspaceViewModel.ProjectName;

            ViewBag.DllStatus = new List<SelectListItem> 
            { 
                new("ToDo", "0"), 
                new("InProgess", "1"), 
                new("Done", "2"),
            };

            
            // workspaceViewModel.ProjectID dan ProjectName akan terisi dari query string
            return View(workspaceViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetKanbanTasks(int projectId)
        {
            var tasks = await _taskRepository.GetAllAsync(projectId);
            return Json(tasks);
        }
    }
}

