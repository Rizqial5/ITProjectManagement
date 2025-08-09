using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models;

namespace ProjectManagement.App.Services.Interfaces
{
    public interface IGithubService
    {
        public string GetCallBackGithub(string clientId, string callBackurl);
        public Task<GithubTokenResponseDto> ConnectGithub(string clientId , string clientSecret, string code);
        public Task<bool> IsGithubTokenValid(GithubAuth existingCreds);
        public Task<GithubRepoResponseDto> GetGithubRepo(string accessToken);

        public Task<ResponseResultDto<CommitCheckResultDto>> CheckLatestCommitAsync(
            string accessToken,
            string owner,
            string repoName,
            string? lastKnownSha);

    }
}
