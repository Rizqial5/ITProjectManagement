using System.Text.Json.Serialization;

namespace ProjectManagement.App.DTO.Github
{
    public class ConnectGithubDto
    {
        [JsonPropertyName("Id")]
        public int RepoId { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Html_Url")]
        public string Html_Url { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; } = string.Empty;
    }
}
