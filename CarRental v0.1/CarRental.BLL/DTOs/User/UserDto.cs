namespace CarRental.BLL.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public DateTime? DriverLicenseExpiry { get; set; }
    public Domain.Enums.UserStatus Status { get; set; }
}
