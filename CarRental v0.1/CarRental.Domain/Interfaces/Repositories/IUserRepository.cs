using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> UserExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByStatusAsync(Enums.UserStatus status);
    Task AddAsync(User entity);
    void Update(User entity);
    void Delete(User entity);
    Task<bool> SaveChangesAsync();
}
