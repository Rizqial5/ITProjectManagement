using Microsoft.AspNetCore.Identity;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<ApplicationUser?> LoginAsync(LoginViewModel model);
        Task LogoutAsync();

        Task<ResponseResultDto<GithubAuth>> SaveOrUpdateGithubCredentials(CreateGithubAuthDto model);
        Task<ResponseResultDto<GithubAuth>> GetGithubCreds(string userId);
        Task<List<ApplicationUser>> GetAllUsersAsync();
    }
}
