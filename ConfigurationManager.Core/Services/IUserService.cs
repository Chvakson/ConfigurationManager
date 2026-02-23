using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string username, string email);
    Task<User?> UpdateUserAsync(Guid id, string? username, string? email);
    Task<bool> DeleteUserAsync(Guid id);
}