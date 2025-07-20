using ProjectManagement.App.Models.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjectManagement.App.DTO.Workspace
{
    public class KanbanDTO
    {
        [Required]
       
        public string Title { get; set; }

        [Required]
        [DisplayName("Description")]
        public string Description  { get; set; }

        [Required]
        [DisplayName("Status")]
        public Status Status{ get; set; }
    }
}
