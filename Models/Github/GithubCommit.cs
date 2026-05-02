using ProjectManagement.App.Models.Workspace;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.App.Models.Github
{
    public class GithubCommit
    {
        [Key]
        public int Id { get; set; }

        //Commmit Info
        public required string Sha {  get; set; }
        public string? Message {  get; set; }
        public required string AuthorName {  get; set; }
        public string? AuthorEmail {  get; set; }
        public DateTime CommitDate {  get; set; }

        // Fk Repo
        public int RepoId { get; set; }
        public GithubRepo Repo { get; set; } = null!;

        // Fk Task
        public int? TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public bool isAssignedTask => TaskId.HasValue;

        public string? DisplaySHA => Sha.Substring(0,7);

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ProjectName { get; set; }
    }
}
