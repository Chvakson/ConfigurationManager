using ConfigurationManager.Db.Models;
using System;
using System.Collections.Generic;
namespace ConfigurationManager.Db.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(Guid id, User user);
        Task<bool> DeleteAsync(Guid id);
    }
}
