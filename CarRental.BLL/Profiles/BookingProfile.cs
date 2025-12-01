using AutoMapper;
using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Profiles;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car));
        
        CreateMap<BookingRequestDto, Booking>();
    }
}