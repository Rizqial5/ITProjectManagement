using System.Security.Claims;

namespace ProjectManagement.App.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsConnectedGithub(this ClaimsPrincipal user)
        {
            return user.HasClaim("GithubConnected", "true");
        }
    }
}
