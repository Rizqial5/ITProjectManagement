using ProjectManagement.App.Models.Github;

namespace ProjectManagement.App.ViewModel
{
    public class TaskViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }
        public int ProjectId { get; set; }
        public int? TotalLinkedCommits { get; set; } = 0;
        public bool? isConnectedRepo { get; set; } = false;
        public DateTime DueDate { get; set; }
        public string LastUpdated { get; set; }
        public string? Priority { get; set; }
        public int? Completion { get; set; } // percent
        public string? AssigneeName { get; set; }
        public string? AssigneeRole { get; set; }
        public bool isRequestHtmx { get; set; }

        public bool InitPage { get; set; } = true;
        public List<GithubCommit> Commits { get; set; }
    }
}
