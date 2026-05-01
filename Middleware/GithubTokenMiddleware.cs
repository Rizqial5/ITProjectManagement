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
            var userName = context.Session.GetString("GitHubUser");
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


                if (context.User.Identity is ClaimsIdentity identity)
                {
                    // Ensure the claim exists or update it
                    var cekOld = identity.FindFirst("GitHubConnected");

                    if (cekOld is not null)
                    {
                        identity.RemoveClaim(cekOld);
                    }
                    identity.AddClaim(new Claim("GitHubConnected", isValid ? "true" : "false"));

                    // Update token and user claims if available
                    var tokenClaim = identity.FindFirst("GitHubToken");
                    if (tokenClaim is not null) identity.RemoveClaim(tokenClaim);

                    if (!string.IsNullOrWhiteSpace(token))
                        identity.AddClaim(new Claim("GitHubToken", token));

                    var userClaim = identity.FindFirst("GitHubUser");
                    if (userClaim is not null) identity.RemoveClaim(userClaim);

                    if (!string.IsNullOrWhiteSpace(userName))
                        identity.AddClaim(new Claim("GitHubUser", userName));
                }
            
            // Update claim


            await next(context);
        }
    }
}
