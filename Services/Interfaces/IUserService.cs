using GestionDesDépenses.DTOs;
using GestionDesDépenses.Models;

namespace GestionDesDépenses.Services.Interfaces
{
    public interface IUserService
    {
        Task EnsureUserExistsAsync(string email, string firstName, string lastName);
        Task<User> GetUserByEmailAsync(string email);
        Task<string> GenerateJwtTokenAsync(User user);
        Task ChangePasswordAsync(string email, ChangePasswordModel model);
        Task ForgotPasswordAsync(ForgotPasswordModel model);
        Task ResetPasswordAsync(ResetPasswordModel model);
    }
}
