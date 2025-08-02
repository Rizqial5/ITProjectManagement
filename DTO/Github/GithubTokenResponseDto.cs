namespace ProjectManagement.App.DTO.Github
{
    public class GithubTokenResponseDto
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }

        // sementara
        public string UserInfo { get; set; }
        public bool IsSuccess {  get; set; }

    }
}
