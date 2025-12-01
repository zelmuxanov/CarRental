using CarRental.BLL.DTOs.Car;

namespace CarRental.BLL.Interfaces.Services;

public interface ICarService
{
    Task<CarDto?> GetCarByIdAsync(Guid id);
    Task<IEnumerable<CarDto>> GetAllCarsAsync();
    Task<IEnumerable<CarDto>> GetAvailableCarsAsync();
    Task<IEnumerable<CarDto>> GetCarsByFilterAsync(CarFilterDto filter);
    Task<CarDto> CreateCarAsync(CarCreateDto createDto);
    Task<bool> UpdateCarAsync(Guid id, CarCreateDto updateDto);
    Task<bool> DeleteCarAsync(Guid id);
    Task<IEnumerable<string>> GetUniqueBrandsAsync();
    Task<IEnumerable<string>> GetUniqueModelsAsync();
    Task<int> GetMinSeatsAsync();
    Task<int> GetMaxSeatsAsync();
}