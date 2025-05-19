using GestionDesDépenses.Models;

namespace GestionDesDépenses.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id);
        Task<List<User>> GetAllUsersAsync();
        Task UpdateAsync(User user);
    }
}
