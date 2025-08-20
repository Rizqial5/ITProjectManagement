using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Services.Response;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IGithubRepository
    {
        Task<ResponseResultDto> InsertGithubCommit(List<GithubCommitApiResponse> newCommits, int repoId);
    }
}
