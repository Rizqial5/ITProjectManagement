using ProjectManagement.App.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.ViewModel.Home
{
    public class ProjectViewModel
    {
        
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public Status Status { get; set; }
        public int TaskTotal { get; set; }
        public int TaskComplete { get; set; }
        public List<string> MemberInitials { get; set; } = new();
    }
}
