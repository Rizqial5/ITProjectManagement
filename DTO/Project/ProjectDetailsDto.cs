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
// Total commits in project
public int TotalCommits { get; set; }

// Use array of objects to store Name and Role
public ProjectMemberDto[] Members { get; set; } = Array.Empty<ProjectMemberDto>();

// Task list for the Project Details view
public ProjectTaskDto[] Tasks { get; set; } = Array.Empty<ProjectTaskDto>();
}

public class ProjectMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
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

        public bool IsOverDue => DateTime.UtcNow > DueDate && Status != "Done";
    }
}