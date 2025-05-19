using GestionDesDépenses.DTOs;
using GestionDesDépenses.Models;

namespace GestionDesDépenses.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterModel model);
        Task<string> LoginAsync(LoginModel model);
        Task LogoutAsync();
        Task UpdateAsync(string userId, UpdateUserModel model);
        Task DeleteAsync(string userId);
        Task ChangePasswordAsync(string userId, ChangePasswordModel model);
        Task<List<User>> GetAllUsersAsync();
        Task LogoutAllUsersAsync(string userId);
    }
}
