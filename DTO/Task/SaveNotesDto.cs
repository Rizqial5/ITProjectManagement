namespace ProjectManagement.App.DTO.Task
{
    public class SaveNotesDto
    {
        public int TaskId {  get; set; }
        public int ProjectId {  get; set; }
        public string NotesHtml {  get; set; }
    }
}
