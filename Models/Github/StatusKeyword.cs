using ProjectManagement.App.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Github
{
    public class StatusKeyword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Keyword { get; set; } = string.Empty;

        [Required]
        public Status TargetStatus { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }
    }
}
