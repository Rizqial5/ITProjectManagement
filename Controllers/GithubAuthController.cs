using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ProjectManagement.App.Controllers
{
    public class GithubAuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public GithubAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("github/connect")]
        public IActionResult Connect()
        {
            var clientId = _configuration["Github:ClientId"];
            var callbackUrl = _configuration["Github:CallbackUrl"];

            var githubOauthUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={callbackUrl}&scope=repo user";

            return Redirect(githubOauthUrl);
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var clientId = _configuration["Github:ClientId"];
            var clientSecret = _configuration["Github:ClientSecret"];

            using var httpClient = new HttpClient();

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string?, string?>("client_id",clientId),
                new KeyValuePair<string?, string?>("client_secret",clientSecret),
                new KeyValuePair<string?, string?>("code",code),
            });

            var response = await httpClient.PostAsync("https://github.com/login/oauth/access_token", requestBody);

            var content = await response.Content.ReadAsStringAsync();

            var parsed = System.Web.HttpUtility.ParseQueryString(content);
            var accessToken = parsed["access_token"];

            if(string.IsNullOrWhiteSpace(accessToken))
            {
                return BadRequest("Failed to obtain access token from github");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ProjectManagementApp", "1.0"));

            var userInfoResponse = await httpClient.GetAsync("https://api.github.com/user");
            var userInfo = await userInfoResponse.Content.ReadAsStringAsync();

            // Simpan accessToken ke session/database atau tampilkan info user
            TempData["GitHubUser"] = userInfo;
            TempData["AccessToken"] = accessToken;


            return RedirectToAction("GitHubConnected");
        }

        [HttpGet("github/connected")]
        public IActionResult GitHubConnected()
        {
            ViewBag.GitHubUser = TempData["GitHubUser"];
            ViewBag.AccessToken = TempData["AccessToken"];
            return View(); // Buat view sederhana untuk tampilkan hasil
        }

    }
}
