using System.Security.Claims;

namespace ProjectManagement.App.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsConnectedGithub(this ClaimsPrincipal user)
        {
            return user.HasClaim("GitHubConnected", "true");
        }

        public static string? GetGithubToken(this ClaimsPrincipal user)
        {
            return user.FindFirst("GitHubToken")?.Value;
        }

        public static string? GetGithubUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst("GitHubUser")?.Value;
        }
    }
}
