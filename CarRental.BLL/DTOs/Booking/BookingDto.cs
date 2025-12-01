namespace CarRental.BLL.DTOs.Booking;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public Domain.Enums.BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационные свойства
    public User.UserDto? User { get; set; }
    public Car.CarDto? Car { get; set; }
}
