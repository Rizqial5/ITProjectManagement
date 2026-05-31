using System.Collections.Generic;

namespace ProjectManagement.App.DTO.Search
{
    public class SearchQueryResultDto
    {
        public List<SearchProjectDto> Projects { get; set; } = new();
        public List<SearchTaskDto> Tasks { get; set; } = new();
        public List<SearchMemberDto> Members { get; set; } = new();
        public string Query { get; set; } = string.Empty;
    }

    public class SearchProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class SearchTaskDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SearchMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
