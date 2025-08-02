using Microsoft.AspNetCore.Identity;
using ProjectManagement.App.Models;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser?> LoginAsync(LoginViewModel model)
        {
            // Cari user berdasarkan Email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return null;

            // Login pakai UserName
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);

            return result.Succeeded ? user : null;
        }

        public async Task LogoutAsync()
        {
             await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,

            };

            return await _userManager.CreateAsync(user, model.Password);
        }

        
    }
}
