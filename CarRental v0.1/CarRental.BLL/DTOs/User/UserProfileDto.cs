namespace CarRental.BLL.DTOs.User;

public class UserProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public DateTime? DriverLicenseExpiry { get; set; }
}
