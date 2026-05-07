using Htmx;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;
using System.Threading.Tasks;
using System.Security.Claims;
using ProjectManagement.App.Extensions;

namespace ProjectManagement.App.Controllers
{
    public class TabsController : Controller
    {
        private readonly ITaskRepository _taskRepository;

        public TabsController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<IActionResult> LoadCommitsTab(int projectId, int taskId, bool isConnected)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return NotFound();

            var taskItem = await _taskRepository.GetAsync(projectId, taskId, workspaceId.Value, userId!);

            if (taskItem == null) return NotFound();

            var countCommit = isConnected ? await _taskRepository.GetTotalIntegratedCommit(projectId, taskId, workspaceId.Value, userId!) : 0;

            var taskModel = new TaskViewModel()
            {
                TaskId = taskItem.Id,
                ProjectId = taskItem.Project.Id,
                TotalLinkedCommits = countCommit,
                Commits = taskItem.Commits.ToList(),
                isConnectedRepo = isConnected,
            };

            return PartialView("~/Views/Shared/Tabs/_LinkedCommitsPanel.cshtml", taskModel);
        }

        public async Task<IActionResult> LoadNotesTab(int projectId, int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return NotFound();

            var notes = await _taskRepository.GetNotesAsync(projectId, taskId, workspaceId.Value, userId!);

            SaveNotesDto modelNotes = new()
            {
                TaskId = taskId,
                ProjectId = projectId,
                NotesHtml = notes
            };

            return PartialView("~/Views/Shared/Tabs/_NotesTab.cshtml", modelNotes);
        }
    }
}
