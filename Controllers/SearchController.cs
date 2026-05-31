using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Repository.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagement.App.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ISearchRepository _searchRepository;

        public SearchController(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Query(string q)
        {
            var workspaceId = User.GetWorkspaceId();
            if (workspaceId == null)
            {
                return Content("<div class='text-center py-3 text-secondary smaller'>No active workspace context.</div>");
            }

            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Content("<div class='text-center py-3 text-secondary smaller'>Type at least 2 characters to search...</div>");
            }

            var results = await _searchRepository.SearchWorkspaceAsync(q, workspaceId.Value);
            return PartialView("_SearchResultsPartial", results);
        }
    }
}
