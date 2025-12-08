using System;

namespace ProjectManagement.App.DTO.Project
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Keep as string to match the usage in Views (e.g. "In Progress", "Completed").
        public string Status { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public bool IsConnected { get; set; }

        // 0-100
        public float Progress { get; set; }

        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int TotalCommits { get; set; }

        // Use array so existing Razor code that uses .Length and LINQ .Any() both work.
        public string[] Members { get; set; } = Array.Empty<string>();

        // Task list for the Project Details view
        public ProjectTaskDto[] Tasks { get; set; } = Array.Empty<ProjectTaskDto>();
    }

    public class ProjectTaskDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Assignee { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        // e.g. "Low", "High"
        public string Priority { get; set; } = string.Empty;

        // e.g. "To Do", "In Progress"
        public string Status { get; set; } = string.Empty;

        public int Commits { get; set; }

        public bool IsOverDue => DateTime.UtcNow < DueDate && Status != "Done";
    }
}