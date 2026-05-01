namespace ProjectManagement.App.ViewModel.Home
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int TotalCompletedTasks { get; set; }
        public IEnumerable<ProjectViewModel> RecentProjects { get; set; } = new List<ProjectViewModel>();
        public IEnumerable<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
    }
}
