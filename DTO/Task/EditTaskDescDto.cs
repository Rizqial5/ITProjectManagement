using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProjectManagement.App.DTO.Task
{
    public class EditTaskDescDto
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }

        
        public required string Description { get; set; }
        public required string Status { get; set; }

        public string? AssignedUserId { get; set; }
        public string? Priority { get; set; }
        public string? LastUpdated { get; set; }
    }
}
