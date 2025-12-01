using Microsoft.AspNetCore.Identity;
using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public DateTime? DriverLicenseExpiry { get; set; }
    public int DrivingExperience { get; set; }
    public UserStatus Status { get; set; }
    public DateTime RegistrationDate { get; set; }

    // Навигационные свойства
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
