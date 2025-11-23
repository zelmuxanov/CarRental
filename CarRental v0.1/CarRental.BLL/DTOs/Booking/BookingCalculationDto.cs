namespace CarRental.BLL.DTOs.Booking;

public class BookingCalculationDto
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal TotalPrice { get; set; }
}
