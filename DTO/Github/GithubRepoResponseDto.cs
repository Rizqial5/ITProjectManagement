using ProjectManagement.App.DTO.Workspace;

namespace ProjectManagement.App.DTO.Github
{
    public class GithubRepoResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public IEnumerable<GitHubRepoDto> GitHubRepos { get; set; }
    }
}
