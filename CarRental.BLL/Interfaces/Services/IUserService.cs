using Microsoft.AspNetCore.Identity;
using CarRental.BLL.DTOs.User;

namespace CarRental.BLL.Interfaces.Services;

public interface IUserService
{
    Task<IdentityResult> CreateUserAsync(RegisterDto registerDto);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserProfileAsync(Guid id, UserProfileDto userProfileDto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> ValidateUserAgeAsync(Guid userId);
    Task<bool> ValidateDrivingExperienceAsync(Guid userId);
    Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<UserVerificationInfoDto> GetUserVerificationInfoAsync(Guid userId);
    Task<UserDocumentsDto> GetUserDocumentsAsync(Guid userId);
    Task<bool> UpdateUserDocumentsAsync(Guid userId, UserDocumentsDto documentsDto);
}
public class UserVerificationInfoDto
{
    public bool IsVerified { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
}