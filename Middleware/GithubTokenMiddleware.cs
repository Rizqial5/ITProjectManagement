using Microsoft.Extensions.Caching.Memory;
using ProjectManagement.App.Services;
using ProjectManagement.App.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManagement.App.Middleware
{
    public class GithubTokenMiddleware : IMiddleware
    {
        
        private readonly IGithubService _authService;
        private readonly IMemoryCache _cache;

        public GithubTokenMiddleware(IGithubService authService, IMemoryCache cache)
        {
         
            _authService = authService;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Session.GetString("GitHubToken");
            bool isValid = false;

            if (!string.IsNullOrWhiteSpace(token)) 
            {
                isValid = await _cache.GetOrCreateAsync($"github_token_valid_{token}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);



                    var isValid= await _authService.IsGithubTokenValid(token);

                    return isValid;
                });
            }

            // Update claim
            if (context.User.Identity is ClaimsIdentity identity)
            {
                var oldClaim = identity.FindFirst("GitHubConnected");
                if (oldClaim != null)
                    identity.RemoveClaim(oldClaim);

                identity.AddClaim(new Claim("GitHubConnected", isValid ? "true" : "false"));
            }

            await next(context);
        }
    }
}
