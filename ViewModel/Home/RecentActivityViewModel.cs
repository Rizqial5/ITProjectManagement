using ProjectManagement.App.Models.Enum;

namespace ProjectManagement.App.ViewModel.Home
{
    public class RecentActivityViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ActivityType { get; set; } = string.Empty; // e.g., "Commit", "Task Completed"
        public Status? Status { get; set; }
    }
}
