using AutoMapper;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.BLL.Utilities;

namespace CarRental.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly PriceCalculator _priceCalculator;

    public BookingService(
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        IUserRepository userRepository,
        IMapper mapper,
        PriceCalculator priceCalculator)
    {
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _priceCalculator = priceCalculator;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        return _mapper.Map<BookingDto?>(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto> CreateBookingAsync(BookingRequestDto requestDto)
    {
        // Проверка доступности автомобиля
        var isAvailable = await _carRepository.IsCarAvailableAsync(
            requestDto.CarId, requestDto.StartDate, requestDto.EndDate);

        if (!isAvailable)
            throw new Domain.Exceptions.BusinessException("CAR_NOT_AVAILABLE", "Car is not available for the selected dates");

        // Расчет цены
        var car = await _carRepository.GetByIdAsync(requestDto.CarId);
        if (car == null)
            throw new Domain.Exceptions.BusinessException("CAR_NOT_FOUND", "Car not found");

        var totalPrice = _priceCalculator.CalculatePrice(car.PricePerDay, requestDto.StartDate, requestDto.EndDate);

        var booking = _mapper.Map<Domain.Entities.Booking>(requestDto);
        booking.TotalPrice = totalPrice;

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return false;

        booking.Status = Domain.Enums.BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        return await _bookingRepository.SaveChangesAsync();
    }

    public async Task<bool> ConfirmBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return false;

        booking.Status = Domain.Enums.BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        return await _bookingRepository.SaveChangesAsync();
    }

    public async Task<BookingCalculationDto> CalculateBookingPriceAsync(BookingRequestDto requestDto)
    {
        var car = await _carRepository.GetByIdAsync(requestDto.CarId);
        if (car == null)
            throw new Domain.Exceptions.BusinessException("CAR_NOT_FOUND", "Car not found");

        var totalPrice = _priceCalculator.CalculatePrice(car.PricePerDay, requestDto.StartDate, requestDto.EndDate);
        var days = (requestDto.EndDate - requestDto.StartDate).Days;

        return new BookingCalculationDto
        {
            CarId = requestDto.CarId,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate,
            Days = days,
            PricePerDay = car.PricePerDay,
            TotalPrice = totalPrice
        };
    }
}
