using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(int projectId);
        Task<TaskItem?> GetAsync(int projectId, int taskId);
        Task AddAsync(int projectId, TaskItem task);
        Task<bool> UpdateAsync(int projectId, TaskItem task);
        Task<bool> DeleteAsync(int projectId, int taskId);

        Task<IEnumerable<CommitDto>> GetAllCommitAsync(int projectId);
        Task<IEnumerable<CommitDto>> GetAllIntegratedCommitAsync(int projectId);
        Task<int> GetTotalIntegratedCommit(int projectId);
        Task<ResponseResultDto> ConnectCommitToTaskAsync(int repoId, int commitId, int taskId);
        Task<ResponseResultDto> SetTaskStatus(int projectId, int taskId, Status setStatus);
    }
}