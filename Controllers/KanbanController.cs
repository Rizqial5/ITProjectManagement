using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;
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
            if (kanban == null || string.IsNullOrEmpty(kanban.Status.ToString()))
            {
                return BadRequest(new { success = false, message = "Data tidak valid." });
            }


            // UPDATE
            var existingTask = await _taskRepository.GetAsync(projectId,kanban.Id);
            if (existingTask == null)
                return NotFound(new { success = false, message = "Data tidak ditemukan." });

            existingTask.Title = kanban.Title;
            existingTask.Description = kanban.Description;
            existingTask.Status = kanban.Status;
            await _taskRepository.UpdateAsync(projectId,existingTask);

            return Ok(new { success = true, message = "Data berhasil diubah." });
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteKanbanData([FromBody] KanbanDTO kanban, int projectId)
        {
            //var task = await _taskRepository.GetByIdAsync(kanban.Id);
            //if (task == null)
            //    return NotFound(new { success = false, message = "Data tidak ditemukan." });

            //await _taskRepository.DeleteAsync(task.Id);
            return Ok(new { success = true, message = "Data berhasil dihapus." });
        }
    }
}   
