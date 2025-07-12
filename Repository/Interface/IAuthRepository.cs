using Microsoft.AspNetCore.Identity;
using ProjectManagement.App.Models;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<ApplicationUser?> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
    }
}
