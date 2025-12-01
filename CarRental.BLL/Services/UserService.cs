using AutoMapper;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarRental.BLL.Services;

    public class UserService : IUserService
    {
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, UserManager<User> userManager, IMapper mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IdentityResult> CreateUserAsync(RegisterDto registerDto)
    {
        // Проверяем обязательные поля
        if (!registerDto.BirthDate.HasValue)
            return IdentityResult.Failed(new IdentityError { Description = "Дата рождения обязательна" });

        if (!registerDto.DrivingExperience.HasValue)
            return IdentityResult.Failed(new IdentityError { Description = "Стаж вождения обязателен" });

        // Проверяем возраст (23+ лет)
        var age = DateTime.Now.Year - registerDto.BirthDate.Value.Year;
        if (registerDto.BirthDate.Value > DateTime.Now.AddYears(-age)) age--;
        
        if (age < 23)
            return IdentityResult.Failed(new IdentityError { Description = "Минимальный возраст для регистрации - 23 года" });

        // Проверяем стаж (2+ года)
        if (registerDto.DrivingExperience < 2)
            return IdentityResult.Failed(new IdentityError { Description = "Минимальный стаж вождения - 2 года" });

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.Email,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            BirthDate = registerDto.BirthDate.Value,
            DrivingExperience = registerDto.DrivingExperience.Value,
            Status = UserStatus.Pending,
            RegistrationDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        return result;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;
        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<bool> UpdateUserProfileAsync(Guid id, UserProfileDto userProfileDto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        user.FirstName = userProfileDto.FirstName;
        user.LastName = userProfileDto.LastName;
        user.PhoneNumber = userProfileDto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ValidateUserAgeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var age = DateTime.Now.Year - user.BirthDate.Year;
        return age >= 23;
    }

    public async Task<bool> ValidateDrivingExperienceAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        return user.DrivingExperience >= 2;
    }
    public async Task<bool> UpdateUserAsync(Guid id, UserProfileDto userProfileDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.FirstName = userProfileDto.FirstName;
        user.LastName = userProfileDto.LastName;
        user.PhoneNumber = userProfileDto.PhoneNumber;

        _userRepository.Update(user);
        return await _userRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        _userRepository.Delete(user);
        return await _userRepository.SaveChangesAsync();
    }
    public async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Пользователь не найден" });

        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<UserVerificationInfoDto> GetUserVerificationInfoAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) 
            return new UserVerificationInfoDto { Status = "Не найден" };

        return new UserVerificationInfoDto
        {
            IsVerified = user.Status == UserStatus.Active,
            Status = user.Status.ToString(),
            VerificationDate = user.UpdatedAt
        };
    }
    public async Task<UserDocumentsDto> GetUserDocumentsAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null) return new UserDocumentsDto();

    return new UserDocumentsDto
    {
        DriverLicenseNumber = user.DriverLicenseNumber,
        DriverLicenseExpiry = user.DriverLicenseExpiry,
        HasDriverLicense = !string.IsNullOrEmpty(user.DriverLicenseNumber),
        DocumentsVerifiedAt = user.UpdatedAt,
        VerificationStatus = user.Status == UserStatus.Active ? "Verified" : "Pending"
    };
}

    public async Task<bool> UpdateUserDocumentsAsync(Guid userId, UserDocumentsDto documentsDto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.DriverLicenseNumber = documentsDto.DriverLicenseNumber;
        user.DriverLicenseExpiry = documentsDto.DriverLicenseExpiry;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}