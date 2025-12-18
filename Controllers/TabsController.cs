using Htmx;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;
using System.Threading.Tasks;

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
            var taskItem = await _taskRepository.GetAsync(projectId, taskId);
            var countCommit = isConnected ? await _taskRepository.GetTotalIntegratedCommit(projectId, taskId) : 0;

            //if (taskItem == null)
            //{
            //    TempData["RepoNotificationFailed"] = "Task is not exists";
            //    return RedirectToAction("Index", "Workspace", new { ProjectID = projectId });
            //}

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

        public IActionResult LoadNotesTab(int projectId, int taskId)
        {

            return PartialView("~/Views/Shared/Tabs/_NotesTab.cshtml");
        }
    }
}
