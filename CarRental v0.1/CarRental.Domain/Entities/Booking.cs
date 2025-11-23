using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }

    // Навигационные свойства
    public virtual User User { get; set; } = null!;
    public virtual Car Car { get; set; } = null!;
}
