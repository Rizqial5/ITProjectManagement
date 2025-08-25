using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.DTO.Task
{
    public class ExistCommitDto
    {
        
        public int No { get; set; }
        public string Message { get; set; }

        [Display(Name = "Author Name")]
        public string AuthorName { get; set; }


        [Display(Name = "Commit Date")]
        public DateTime CommitDate { get; set; }
    }
}
