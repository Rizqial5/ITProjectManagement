using Azure.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Services.Interfaces;
using ProjectManagement.App.Services.Response;
using Syncfusion.EJ2.Base;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjectManagement.App.Services
{
    public class GithubService : IGithubService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GithubService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseResultDto<CommitCheckResultDto>> CheckLatestCommitAsync(string accessToken, string owner, string repoName, string? lastKnownSha)
        {
            var client = _httpClientFactory.CreateClient("Github");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ProjectManagementApp/1.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // ambil commit terbaru (per_page=1 cepat)
            var url = $"repos/{owner}/{repoName}/commits?per_page=1";
            var resp = await client.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                return new()
                {
                    Success = false,
                    Message = $"GitHub API error: {resp.StatusCode} {resp.ReasonPhrase}"
                };
            }

            var json = await resp.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<GithubCommitDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (commits == null || commits.Count == 0)
            {
                return new() 
                { 
                    Success = true,  
                    Message = "No commits." 
                };
            }

            var latest = commits[0];
            var hasNew = string.IsNullOrEmpty(lastKnownSha) ? false : latest.Sha != lastKnownSha;

            // NOTE: if you want to treat "no last sha" as new, set hasNew = string.IsNullOrEmpty(lastKnownSha) ? true : (latest.Sha != lastKnownSha);

            var data = new CommitCheckResultDto
            {
                HasNewCommit = hasNew,
                LatestCommitSha = latest.Sha,
                //LatestCommitMessage = latest.Commit.Message,
                CommitUrl = latest.HtmlUrl
            };

            return new()
            {
                Success = true,
                Data = data
            };
        }

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

        public async Task<bool> IsGithubTokenValid(GithubAuth existingCreds)
        {
            var client = _httpClientFactory.CreateClient("Github");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", existingCreds.AccessToken);

            var response = await client.GetAsync("user");

            return response.IsSuccessStatusCode;
        }

        public async Task<ResponseResultDto<List<GithubCommitApiResponse>>> GetCommitsAsync(GitHubRepoDto repoData , string accessToken)
        {
            var client = _httpClientFactory.CreateClient("Github");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ProjectManagementApp/1.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string url = string.Empty;
            if(repoData.LastKnownCommitDate == null)
            {
                url = $"repos/{repoData.RepoOwner}/{repoData.Name}/commits?per_page=50";
            }
            else
            {
                var since = repoData.LastKnownCommitDate.Value.UtcDateTime.ToString("o");
                url = $"repos/{repoData.RepoOwner}/{repoData.Name}/commits?per_page=50";
            }

            //call api
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new()
                {
                    Success = false,
                    Message = response.ReasonPhrase ?? ""
                };
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var commitData = JsonSerializer.Deserialize<List<GithubCommitApiResponse>>(json, options) ?? Enumerable.Empty<GithubCommitApiResponse>();

            if (!commitData.Any()) 
            {
                return new()
                {
                    Success = false,
                    Message = "Data is already up to date"
                };
            }

            return new()
            {
                Success = true,
                Data = commitData.ToList()
            };

        }
    }
}
