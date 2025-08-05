namespace ProjectManagement.App.DTO.Github
{
    public class CreateGithubAuthDto
    {

        public long GitHubId { get; set; }
        public string GitHubUsername { get; set; }
        public string GitHubEmail { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }

        public string UserId { get; set; }
    }
}
