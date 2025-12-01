using AutoMapper;
using CarRental.Domain.Entities;
using CarRental.BLL.DTOs.Car;
using CarRental.BLL.DTOs.User;
using CarRental.BLL.DTOs.Booking;

namespace CarRental.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserProfileDto, User>();

        // Car mappings
        CreateMap<Car, CarDto>();
        CreateMap<CarCreateDto, Car>();

        // Booking mappings
        CreateMap<Booking, BookingDto>();
        CreateMap<BookingRequestDto, Booking>();
    }
}
