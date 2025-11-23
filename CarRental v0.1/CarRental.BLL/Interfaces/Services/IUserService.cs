using CarRental.BLL.DTOs.User;

namespace CarRental.BLL.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserProfileAsync(Guid userId, UserProfileDto updateDto);
    Task<bool> DeleteUserAsync(Guid id);
}
