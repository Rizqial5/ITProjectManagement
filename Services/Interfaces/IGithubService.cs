using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.DTO.Workspace;

namespace ProjectManagement.App.Services.Interfaces
{
    public interface IGithubService
    {
        public string GetCallBackGithub(string clientId, string callBackurl);
        public Task<GithubTokenResponseDto> ConnectGithub(string clientId , string clientSecret, string code);
        public Task<GithubRepoResponseDto> GetGithubRepo(string accessToken);

    }
}
