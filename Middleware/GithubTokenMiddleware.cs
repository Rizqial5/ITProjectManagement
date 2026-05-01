using Microsoft.Extensions.Caching.Memory;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services;
using ProjectManagement.App.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManagement.App.Middleware
{
    public class GithubTokenMiddleware : IMiddleware
    {
        
        private readonly IGithubService _authService;
        private readonly IMemoryCache _cache;
        private readonly IAuthRepository _authRepository;

        public GithubTokenMiddleware(IGithubService authService, IMemoryCache cache, IAuthRepository authRepository)
        {
         
            _authService = authService;
            _cache = cache;
            _authRepository = authRepository;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Session.GetString("GitHubToken");
            var userName = context.Session.GetString("GitHubUser");

            // If session is empty but user is authenticated, try to restore from claims or DB
            if (string.IsNullOrEmpty(token) && context.User.Identity?.IsAuthenticated == true)
            {
                token = context.User.FindFirst("GitHubToken")?.Value;
                userName = context.User.FindFirst("GitHubUser")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var creds = await _authRepository.GetGithubCreds(userId);
                        if (creds.Success && creds.Data != null)
                        {
                            token = creds.Data.AccessToken;
                            userName = creds.Data.GitHubUsername;
                        }
                    }
                }

                // Restore to session if found
                if (!string.IsNullOrEmpty(token))
                {
                    context.Session.SetString("GitHubToken", token);
                    if (!string.IsNullOrEmpty(userName))
                    {
                        context.Session.SetString("GitHubUser", userName);
                    }
                }
            }

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
