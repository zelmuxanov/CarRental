using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class CarRepository : Repository<Car>, ICarRepository
{
    public CarRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
    {
        return await _dbSet.Where(c => c.IsAvailable).ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsByFilterAsync(string? brand, string? model, decimal? maxPrice)
    {
        var query = _dbSet.AsQueryable();
        
        if (!string.IsNullOrEmpty(brand))
            query = query.Where(c => c.Brand.Contains(brand));
            
        if (!string.IsNullOrEmpty(model))
            query = query.Where(c => c.Model.Contains(model));
            
        if (maxPrice.HasValue)
            query = query.Where(c => c.PricePerDay <= maxPrice.Value);
            
        return await query.ToListAsync();
    }

    public async Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate)
    {
        return !await _context.Bookings
            .AnyAsync(b => b.CarId == carId && 
                          b.Status != BookingStatus.Cancelled &&
                          b.Status != BookingStatus.Rejected &&
                          b.StartDate <= endDate && 
                          b.EndDate >= startDate);
    }
}
