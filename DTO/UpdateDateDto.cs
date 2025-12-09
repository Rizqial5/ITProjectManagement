using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO
{
    public class UpdateDateDto
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string? ControllerName { get; set; }


        [DisplayName("New Target Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public  DateTime SetNewDate { get; set; }
    }
}
