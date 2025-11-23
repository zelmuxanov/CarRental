using CarRental.BLL.DTOs.Booking;

namespace CarRental.BLL.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId);
    Task<BookingDto> CreateBookingAsync(BookingRequestDto requestDto);
    Task<bool> CancelBookingAsync(Guid bookingId);
    Task<bool> ConfirmBookingAsync(Guid bookingId);
    Task<BookingCalculationDto> CalculateBookingPriceAsync(BookingRequestDto requestDto);
}
