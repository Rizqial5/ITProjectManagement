using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Interfaces;
using Syncfusion.EJ2.Base;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectManagement.App.Controllers
{
    public class GithubAuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IGithubService _githubService;
        private readonly IAuthRepository _authRepository;

        public GithubAuthController(IConfiguration configuration, 
            IGithubService githubService,
            IAuthRepository authRepository)
        {
            _configuration = configuration;
            _githubService = githubService;
            _authRepository = authRepository;
        }

        [HttpGet("github/connect")]
        public IActionResult Connect()
        {
            var clientId = _configuration["Github:ClientId"];
            var callbackUrl = _configuration["Github:CallbackUrl"];

            var githubOauthUrl = _githubService.GetCallBackGithub(clientId, callbackUrl);

            return Redirect(githubOauthUrl);
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var clientId = _configuration["Github:ClientId"];
            var clientSecret = _configuration["Github:ClientSecret"];
            GithubAuth result;

           
            var tokenResponse = await _githubService.ConnectGithub(clientId, clientSecret,code);

            // Simpan accessToken ke session/database atau tampilkan info user

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var check = await _authRepository.CheckGithubcredentials(userId);

            //check apakah token masih valid

            if(!check.Success)
            {
                result = await SaveGithubCredentials(tokenResponse, userId);
            }
            else
            {
                var isValid = await _githubService.IsGithubTokenValid(check.Data);

                if (isValid) 
                {
                    result = check.Data;
                }
                else
                {
                    result = await SaveGithubCredentials(tokenResponse, userId);
                }
                

                    
            }


            TempData["GitHubUser"] = result.GitHubId.ToString();
            TempData["AccessToken"] = result.AccessToken;


            return RedirectToAction("GitHubConnected");
        }

        private async Task<GithubAuth> SaveGithubCredentials(GithubTokenResponseDto tokenResponse, string userId)
        {
            var model = new CreateGithubAuthDto()
            {
                AccessToken = tokenResponse.AccessToken,
                GitHubId = tokenResponse.UserInfo.Id,
                GitHubUsername = tokenResponse.UserInfo.Login,
                UserId = userId
            };

            var responseResult = await _authRepository.SaveGithubCredentials(model);

            var result = responseResult.Data;
            return result;
        }

        [HttpGet("github/connected")]
        public IActionResult GitHubConnected()
        {
            var githubUsername = TempData["GitHubUser"] as string;
            var accessToken = TempData["AccessToken"] as string;

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(githubUsername))
            {
                HttpContext.Session.SetString("GitHubToken", accessToken);
                HttpContext.Session.SetString("GitHubUser", githubUsername);

            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ShowGithubRepo([FromBody] DataManagerRequest DataManagerRequest)
        {
            var token = HttpContext.Session.GetString("GitHubToken");

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { result = new List<object>(), count = 0 });
            }

            var response = await _githubService.GetGithubRepo(token);

            if(!response.IsSuccess)
            {
                return Json(new { result = new List<object>(), count = 0 });
            }


            DataOperations dataOperations = new();

            var result = dataOperations.Execute(response.GitHubRepos, DataManagerRequest);

            return Json(new { result = result, count = response.GitHubRepos.Count() });
        }

    }
}
