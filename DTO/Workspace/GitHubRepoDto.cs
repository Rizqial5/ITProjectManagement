using System.Text.Json.Serialization;

namespace ProjectManagement.App.DTO.Workspace
{
    public class GitHubRepoDto
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Html_Url")]
        public string Html_Url { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }
    }
}
