using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Workspace;
using System.Security.Claims;
using ProjectManagement.App.Extensions;

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
            return View(workspaceViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetKanbanTasks(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return Json(new List<KanbanDTO>());

            var tasks = await _taskRepository.GetAllAsync(projectId, workspaceId.Value, userId!);

            var kanbanData = tasks.Select(i => new KanbanDTO
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Status = i.Status,
            });

            return Json(kanbanData);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateKanbanData([FromBody] KanbanDTO kanban, int projectId)
        {
            return Ok(new { success = true, message = "Data berhasil diubah." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteKanbanData([FromBody] KanbanDTO kanban, int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return Json(new { success = false, message = "Workspace not found." });

            var task = await _taskRepository.GetAsync(projectId, kanban.Id, workspaceId.Value, userId!);
            if (task == null)
                return NotFound(new { success = false, message = "Data tidak ditemukan." });

            await _taskRepository.DeleteAsync(projectId, task.Id, workspaceId.Value, userId!);
            return Ok(new { success = true, message = "Data berhasil dihapus." });
        }
    }
}
