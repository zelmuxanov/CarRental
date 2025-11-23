using AutoMapper;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Car;
using CarRental.Domain.Interfaces.Repositories;

namespace CarRental.BLL.Services;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;

    public CarService(ICarRepository carRepository, IMapper mapper)
    {
        _carRepository = carRepository;
        _mapper = mapper;
    }

    public async Task<CarDto?> GetCarByIdAsync(Guid id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        return _mapper.Map<CarDto?>(car);
    }

    public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
    {
        var cars = await _carRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CarDto>>(cars);
    }

    public async Task<IEnumerable<CarDto>> GetAvailableCarsAsync()
    {
        var cars = await _carRepository.GetAvailableCarsAsync();
        return _mapper.Map<IEnumerable<CarDto>>(cars);
    }


    public async Task<IEnumerable<CarDto>> GetCarsByFilterAsync(CarFilterDto filter)
    {
        var cars = await _carRepository.GetCarsByFilterAsync(
            filter.Brand, 
            filter.Model, 
            filter.MaxPrice
        );
        return _mapper.Map<IEnumerable<CarDto>>(cars);
    }

    public async Task<CarDto> CreateCarAsync(CarCreateDto createDto)
    {
        var car = _mapper.Map<Domain.Entities.Car>(createDto);
        await _carRepository.AddAsync(car);
        await _carRepository.SaveChangesAsync();
        
        return _mapper.Map<CarDto>(car);
    }

    public async Task<bool> UpdateCarAsync(Guid id, CarCreateDto updateDto)
    {
        var car = await _carRepository.GetByIdAsync(id);
        if (car == null) return false;

        _mapper.Map(updateDto, car);
        car.UpdatedAt = DateTime.UtcNow;
        
        _carRepository.Update(car);
        return await _carRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteCarAsync(Guid id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        if (car == null) return false;

        _carRepository.Delete(car);
        return await _carRepository.SaveChangesAsync();
    }
}