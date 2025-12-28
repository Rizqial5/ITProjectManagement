using ProjectManagement.App.Helpers;

namespace ProjectManagement.App.DTO.Task
{
    public class SaveNotesDto
    {
        private string _notesHtml;
        public int TaskId {  get; set; }
        public int ProjectId {  get; set; }
        public string NotesHtml
        {
            get => _notesHtml;
            set => _notesHtml = HtmlSanitizeHelper.Sanitize(value);
        }
    }
}
