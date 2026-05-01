using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Repository.Interface;
using System.Security.Claims;

namespace ProjectManagement.App.Filters
{
    public class ProjectAuthorizeAttribute : TypeFilterAttribute
    {
        public ProjectAuthorizeAttribute(params ProjectRole[] roles) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class ProjectAuthorizeFilter : IAsyncActionFilter
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ProjectRole[] _roles;

        public ProjectAuthorizeFilter(IProjectRepository projectRepository, ProjectRole[] roles)
        {
            _projectRepository = projectRepository;
            _roles = roles;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Look for projectId in route data or query string
            object? projectIdObj = null;

            if (context.RouteData.Values.TryGetValue("projectId", out projectIdObj) ||
                context.RouteData.Values.TryGetValue("id", out projectIdObj))
            {
                // Found in route data
            }
            else if (context.HttpContext.Request.Query.TryGetValue("projectId", out var queryVal))
            {
                projectIdObj = queryVal.ToString();
            }
            else if (context.HttpContext.Request.HasFormContentType && 
                     context.HttpContext.Request.Form.TryGetValue("projectId", out var formVal))
            {
                projectIdObj = formVal.ToString();
            }

            if (projectIdObj != null && int.TryParse(projectIdObj.ToString(), out int projectId))
            {
                var isAuthorized = await _projectRepository.IsUserAuthorizedAsync(projectId, userId, _roles);
                
                if (!isAuthorized)
                {
                    if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        context.HttpContext.Request.Headers.ContainsKey("HX-Request"))
                    {
                        context.Result = new JsonResult(new { success = false, message = "You do not have permission to perform this action." }) { StatusCode = 403 };
                    }
                    else
                    {
                        context.Result = new ForbidResult();
                    }
                    return;
                }
            }

            await next();
        }
    }
}
