namespace ProjectManagement.App.DTO.Task
{
    public class CommitDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public DateTime CommitDate { get; set; }
        public bool IsIntegrated { get; set; }
    }
}
