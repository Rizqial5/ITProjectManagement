using System.Threading.Tasks;
using ProjectManagement.App.DTO.Search;

namespace ProjectManagement.App.Repository.Interface
{
    public interface ISearchRepository
    {
        Task<SearchQueryResultDto> SearchWorkspaceAsync(string query, int workspaceId);
    }
}
