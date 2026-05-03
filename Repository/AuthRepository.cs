using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Github;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Github;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Interfaces;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGithubService _githubService;
        private readonly IDataProtector _protector;

        private readonly AppDbContext _dbContext;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IInviteRepository _inviteRepository;

        public AuthRepository(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext dbContext,
            IDataProtectionProvider protector,
            IGithubService githubService,
            IWorkspaceRepository workspaceRepository,
            IInviteRepository inviteRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _protector = protector.CreateProtector("GithubTokenProtector");
            _githubService = githubService;
            _workspaceRepository = workspaceRepository;
            _inviteRepository = inviteRepository;
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
            ApplicationUser? user;

            // Check if input is email
            if (model.UsernameOrEmail.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.UsernameOrEmail);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            }

            if (user == null)
                return null;

            // Login using UserName and Password
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, false);

            return result.Succeeded ? user : null;
        }

        public async Task LogoutAsync()
        {
             await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Email '{model.Email}' is already registered." });
            }

            var existingUserByName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByName != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Username '{model.UserName}' is already taken." });
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Check for and accept any pending invites for this email
                        await _inviteRepository.AcceptPendingInvitesForUser(user.Email, user.Id);

                        // If user wasn't added to a workspace via invite, create a default one
                        var isInAnyWorkspace = await _dbContext.WorkspaceMembers.AnyAsync(wm => wm.UserId == user.Id);
                        if (!isInAnyWorkspace)
                        {
                            await _workspaceRepository.CreateDefaultWorkspaceAsync(user.Id, user.UserName);
                        }
                        
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return IdentityResult.Failed(new IdentityError { Description = $"An error occurred during registration: {ex.Message}" });
                }
            });
        }

        public async Task<ResponseResultDto<GithubAuth>> SaveOrUpdateGithubCredentials(CreateGithubAuthDto model)
        {

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var existing = await _dbContext.GithubAuths.FirstOrDefaultAsync(i => i.UserId == model.UserId);
                if (existing == null)
                {
                    // Simpan baru
                    var entity = new GithubAuth
                    {
                        AccessToken = _protector.Protect(model.AccessToken),
                        GitHubId = model.GitHubId,
                        GitHubUsername = model.GitHubUsername,
                        UserId = model.UserId
                    };
                    _dbContext.GithubAuths.Add(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new() { Success = true, Data = entity };
                }
                else
                {
                    // Update jika token tidak valid
                    var token = _protector.Unprotect(existing.AccessToken);
                    var isValid = await _githubService.IsGithubTokenValid(token);
                    if (!isValid)
                    {
                        existing.AccessToken = _protector.Protect(model.AccessToken);
                        existing.GitHubUsername = model.GitHubUsername;
                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    return new() { Success = true, Data = existing };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new() { Success = false, Message = ex.Message };
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
