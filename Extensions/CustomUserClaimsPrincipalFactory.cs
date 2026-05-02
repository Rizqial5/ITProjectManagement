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

        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IWorkspaceRepository workspaceRepository)
            : base(userManager, roleManager, optionsAccessor)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Get the user's workspaces
            var workspaces = await _workspaceRepository.GetUserWorkspacesAsync(user.Id);
            var firstWorkspace = workspaces.FirstOrDefault();

            if (firstWorkspace != null)
            {
                // Add WorkspaceId as a claim
                identity.AddClaim(new Claim("WorkspaceId", firstWorkspace.Id.ToString()));
                identity.AddClaim(new Claim("WorkspaceName", firstWorkspace.Name));
            }

            return identity;
        }
    }
}
