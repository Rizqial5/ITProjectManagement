using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
using System.Security.Claims;

namespace ProjectManagement.App.Extensions
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IWorkspaceRepository workspaceRepository,
            IHttpContextAccessor httpContextAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _workspaceRepository = workspaceRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Get the user's workspaces
            var workspaces = await _workspaceRepository.GetUserWorkspacesAsync(user.Id);
            var firstWorkspaceMember = workspaces.FirstOrDefault();

            // Try to retrieve active workspace from session
            int? activeWorkspaceId = null;
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                activeWorkspaceId = httpContext.Session.GetInt32("ActiveWorkspaceId");
            }

            var activeWorkspaceMember = activeWorkspaceId.HasValue
                ? workspaces.FirstOrDefault(wm => wm.WorkspaceId == activeWorkspaceId.Value)
                : null;

            var selectedWorkspaceMember = activeWorkspaceMember ?? firstWorkspaceMember;

            if (selectedWorkspaceMember != null && selectedWorkspaceMember.Workspace != null)
            {
                // Add WorkspaceId as a claim
                identity.AddClaim(new Claim("WorkspaceId", selectedWorkspaceMember.WorkspaceId.ToString()));
                identity.AddClaim(new Claim("WorkspaceName", selectedWorkspaceMember.Workspace.Name));
            }

            return identity;
        }
    }
}
