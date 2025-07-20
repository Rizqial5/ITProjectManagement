using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Workspace;

namespace ProjectManagement.App.Controllers
{
    public class KanbanController : Controller
    {

        private readonly ITaskRepository _taskRepository;

        public KanbanController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public IActionResult KanbanView(WorkspaceViewModel workspaceViewModel)
        {
            // workspaceViewModel.ProjectID dan ProjectName akan terisi dari query string
            return View(workspaceViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetKanbanTasks(int projectId)
        {
            var tasks = await _taskRepository.GetAllAsync(projectId);

            var kanbanData = tasks.Select(i => new KanbanDTO
            {
                Title = i.Title,
                Description = i.Description,
                Status = i.Status,
            });

            return Json(kanbanData);
        }
    }
}   
