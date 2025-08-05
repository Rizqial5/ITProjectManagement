using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Services.Interfaces;
using Syncfusion.EJ2.Base;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjectManagement.App.Services
{
    public class GithubService : IGithubService
    {
        public async Task<GithubTokenResponseDto> ConnectGithub(string clientId, string clientSecret, string code)
        {
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

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new GithubTokenResponseDto() { IsSuccess = false };
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ProjectManagementApp", "1.0"));

            var userInfoResponse = await httpClient.GetAsync("https://api.github.com/user");
            var userInfo = await userInfoResponse.Content.ReadAsStringAsync();

            var userDto = JsonSerializer.Deserialize<GitHubUserDto>(userInfo, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new GithubTokenResponseDto()
            {
                AccessToken = accessToken,
                IsSuccess = true,
                UserInfo = userDto
            };

        }

        public string GetCallBackGithub(string clientId, string callBackurl)
        {
            return $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={callBackurl}&scope=repo user";
        }

        public async Task<GithubRepoResponseDto> GetGithubRepo(string accessToken)
        {
            using var client = new HttpClient();
            // WAJIB untuk GitHub API
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Project It Apps/1.0"); // <- Tambahkan ini

            var response = await client.GetAsync("https://api.github.com/user/repos");

            if (!response.IsSuccessStatusCode)
            {
                return new GithubRepoResponseDto()
                {
                    IsSuccess = false,
                    Message = response.ReasonPhrase,
                    GitHubRepos = Enumerable.Empty<GitHubRepoDto>()
                };
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var repos = JsonSerializer.Deserialize<List<GitHubRepoDto>>(json, options);

            return new GithubRepoResponseDto()
            {
                IsSuccess = true,
                GitHubRepos = repos
            };

        }
    }
}
