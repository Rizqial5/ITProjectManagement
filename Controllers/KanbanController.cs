using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.ViewModel.Workspace;

namespace ProjectManagement.App.Controllers
{
    public class KanbanController : Controller
    {
        public IActionResult KanbanView(WorkspaceViewModel workspaceViewModel)
        {
            // workspaceViewModel.ProjectID dan ProjectName akan terisi dari query string
            return View(workspaceViewModel);
        }
    }
}   
