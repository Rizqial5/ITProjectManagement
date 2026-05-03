using ProjectManagement.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.App.Repository.Interface
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<List<ApplicationUser>> GetAllUsersAsync();
    }
}
