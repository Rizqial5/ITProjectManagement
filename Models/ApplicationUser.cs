using Microsoft.AspNetCore.Identity;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        // Relasi ke task yang di-assign ke user ini
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
