using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDataProtector _protector;

        private readonly AppDbContext _dbContext;




        public AuthRepository(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            AppDbContext dbContext, IDataProtectionProvider protector)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _protector = protector.CreateProtector("GithubTokenProtector");
        }

        public async Task<ResponseResultDto<GithubAuth>> GetGithubCreds(string userId)
        {
            try
            {
                var githubCredentials = await _dbContext.GithubAuths.FirstOrDefaultAsync(i => i.UserId == userId);

                if(githubCredentials == null)
                {
                    return new()
                    {
                        Success = false,
                        Message = "User Not Found"
                    };
                }

                githubCredentials.AccessToken = _protector.Unprotect(githubCredentials.AccessToken);

                return new()
                {
                    Success = true,
                    Data = githubCredentials
                    
                };


            }
            catch(Exception ex)
            {
                return new()
                {
                    Success = false,
                    Message = ex.Message

                };
            }
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

        public async Task<ResponseResultDto<GithubAuth>> SaveGithubCredentials(CreateGithubAuthDto model)
        {

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await DeleteGithubCreds(model);

                var enryptedToken = _protector.Protect(model.AccessToken);

                var userGihthub = new GithubAuth()
                {
                    GitHubId = model.GitHubId,
                    AccessToken = enryptedToken,
                    GitHubUsername = model.GitHubUsername,
                    UserId = model.UserId,
                    TokenType = model.TokenType
                };

            
                await _dbContext.GithubAuths.AddAsync(userGihthub);

                await _dbContext.SaveChangesAsync();

                userGihthub.AccessToken = _protector.Unprotect(userGihthub.AccessToken);

                await transaction.CommitAsync();

                return new()
                {
                    Success = true,
                    Data = userGihthub,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new()
                {
                    Success = false,
                    Message = ex.Message,
                };
            }


        }

        private async Task DeleteGithubCreds(CreateGithubAuthDto model)
        {
            var existingCreds = await _dbContext.GithubAuths.FirstOrDefaultAsync(i => i.UserId == model.UserId);

            if (existingCreds != null)
            {
                _dbContext.GithubAuths.Remove(existingCreds);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
