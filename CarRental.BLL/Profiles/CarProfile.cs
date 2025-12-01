using AutoMapper;
using CarRental.BLL.DTOs.Car;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Profiles;

public class CarProfile : Profile
{
    public CarProfile()
    {
        CreateMap<Car, CarDto>().ReverseMap();
        CreateMap<CarCreateDto, Car>();
    }
}