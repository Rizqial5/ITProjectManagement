namespace ProjectManagement.App.Services.Response
{
    public class GithubCommitApiResponse
    {
        public string Sha { get; set; }
        public CommitInfo Commit { get; set; }
    }

    public class CommitInfo
    {
        public CommitAuthor Author { get; set; }
        public string Message { get; set; }
    }

    public class CommitAuthor
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }
}
