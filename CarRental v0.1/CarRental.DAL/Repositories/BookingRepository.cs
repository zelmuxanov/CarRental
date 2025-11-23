using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
    {
        return await _dbSet
            .Include(b => b.Car)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCarAsync(Guid carId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Where(b => b.CarId == carId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Car)
            .Where(b => b.Status == status)
            .ToListAsync();
    }

    public async Task<bool> HasActiveBookingAsync(Guid userId)
    {
        return await _dbSet.AnyAsync(b =>
            b.UserId == userId &&
            (b.Status == BookingStatus.Active ||
             b.Status == BookingStatus.Confirmed));
    }
}
