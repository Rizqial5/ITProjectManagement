using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.ViewModel.Workspace;

namespace ProjectManagement.App.Controllers
{
    public class WorkspaceController : Controller
    {
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
            // workspaceViewModel.ProjectID dan ProjectName akan terisi dari query string
            return View(workspaceViewModel);
        }


    }
}
