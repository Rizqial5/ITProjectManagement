using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Models.Workspace;

namespace ProjectManagement.App.Repository.Interface
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(int projectId, string userId);
        Task<TaskItem?> GetAsync(int projectId, int taskId, string userId);
        Task AddAsync(int projectId, CreateTaskDto task, string userId);
        Task<bool> UpdateAsync(int projectId, TaskItem task, string userId);
        Task<bool> DeleteAsync(int projectId, int taskId, string userId);

        Task<IEnumerable<GithubCommit>> GetLinkedCommit(int repoId, int taskId, string userId);

        Task<IEnumerable<CommitDto>> GetAllCommitAsync(int projectId, string userId);
        Task<IEnumerable<CommitDto>> GetAllIntegratedCommitAsync(int projectId, int taskId, string userId);
        Task<int> GetTotalIntegratedCommit(int projectId, int taskId, string userId);
        Task<ResponseResultDto> ConnectCommitToTaskAsync(int repoId, int commitId, int taskId, string userId);
        Task<ResponseResultDto> SetTaskStatus(int projectId, int taskId, Status setStatus, string userId);
        Task<bool> UpdateDateAsync(TaskItem updatedData, string userId);
        Task AddNoteAsync(SaveNotesDto notesDto, string userId);
        Task<string?> GetNotesAsync(int projectId, int taskId, string userId);
    }
}