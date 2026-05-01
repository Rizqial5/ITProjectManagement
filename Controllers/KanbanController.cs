using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel.Workspace;
using System.Security.Claims;

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
            var tasks = await _taskRepository.GetAllAsync(projectId, userId);

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
            var task = await _taskRepository.GetAsync(projectId, kanban.Id, userId);
            if (task == null)
                return NotFound(new { success = false, message = "Data tidak ditemukan." });

            await _taskRepository.DeleteAsync(projectId, task.Id, userId);
            return Ok(new { success = true, message = "Data berhasil dihapus." });
        }
    }
}
