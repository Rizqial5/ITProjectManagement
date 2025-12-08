using ProjectManagement.App.Models.Enum;

namespace ProjectManagement.App.ViewModel.Project
{
    public class ProjectCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Status Status { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public float Progress => TotalTasks != 0 ? ((float)CompletedTasks / TotalTasks) * 100 : 0;
        public DateTime DueDate { get; set; }
        public bool IsConnected { get; set; }

        public int RepoId { get; set; }
        //public List<MemberViewModel> Members { get; set; } = new();
    }
}
