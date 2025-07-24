using System.Text.Json.Serialization;

namespace ProjectManagement.App.DTO.Workspace
{
    public class GitHubRepoDto
    {
        public string Name { get; set; }

        [JsonPropertyName("html_url")]
        public string Html_Url { get; set; }
        public string Description { get; set; }
    }
}
