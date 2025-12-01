using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface ICarRepository : IRepository<Car>
{
    Task<IEnumerable<Car>> GetAvailableCarsAsync();
    Task<IEnumerable<Car>> GetCarsByFilterAsync(string? brand, string? model, decimal? maxPrice);
    Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<string>> GetUniqueBrandsAsync();
    Task<IEnumerable<string>> GetUniqueModelsAsync();
    Task<int> GetMinSeatsAsync();
    Task<int> GetMaxSeatsAsync();
}