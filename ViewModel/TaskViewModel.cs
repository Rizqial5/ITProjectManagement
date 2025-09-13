namespace ProjectManagement.App.ViewModel
{
    public class TaskViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int? TotalLinkedCommits { get; set; } = 0;
    }
}
