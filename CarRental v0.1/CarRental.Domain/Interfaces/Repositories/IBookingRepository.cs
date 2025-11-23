using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);
    Task<IEnumerable<Booking>> GetBookingsByCarAsync(Guid carId);
    Task<IEnumerable<Booking>> GetBookingsByStatusAsync(Enums.BookingStatus status);
    Task<bool> HasActiveBookingAsync(Guid userId);
}
