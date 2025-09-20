using Microsoft.AspNetCore.Mvc;

namespace ProjectManagement.App.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Page()
        {
            return View();
        }
    }
}
