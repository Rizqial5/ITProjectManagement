using Microsoft.AspNetCore.Mvc;

namespace ProjectManagement.App.Controllers
{
    public class TaskController : Controller
    {
        [HttpGet("/project/{projectId}/task/{taskId}")]
        public IActionResult Details(int projectId, int taskId)
        {
            return View();
        }
    }
}
