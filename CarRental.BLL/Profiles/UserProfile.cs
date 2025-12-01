using AutoMapper;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserProfileDto, User>();
    }
}