namespace ProjectManagement.App.DTO.Github
{
    public class CommitCheckResultDto
    {
        public bool HasNewCommit { get; set; }
        public string? LatestCommitSha { get; set; }
        public string? LatestCommitMessage { get; set; }
        public string? CommitUrl { get; set; }
    }
}
