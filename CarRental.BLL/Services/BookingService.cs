using AutoMapper;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Interfaces.Repositories;

namespace CarRental.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public BookingService(
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<BookingDto> CreateBookingAsync(BookingRequestDto requestDto)
    {
        try
        {
            Console.WriteLine($"🔨 Создание бронирования в BookingService:");
            Console.WriteLine($"  CarId: {requestDto.CarId}");
            Console.WriteLine($"  UserId: {requestDto.UserId}");
            Console.WriteLine($"  StartDate: {requestDto.StartDate:yyyy-MM-dd}");
            Console.WriteLine($"  EndDate: {requestDto.EndDate:yyyy-MM-dd}");
            Console.WriteLine($"  TotalPrice: {requestDto.TotalPrice}₽");
            Console.WriteLine($"  Notes: {requestDto.Notes}");

            // Проверяем существование автомобиля
            var car = await _carRepository.GetByIdAsync(requestDto.CarId);
            if (car == null)
            {
                Console.WriteLine("❌ Автомобиль не найден в репозитории");
                throw new InvalidOperationException("Автомобиль не найден");
            }

            Console.WriteLine($"✅ Автомобиль найден: {car.Brand} {car.Model}");

            // Проверяем существование пользователя
            var user = await _userRepository.GetByIdAsync(requestDto.UserId);
            if (user == null)
            {
                Console.WriteLine("❌ Пользователь не найден в репозитории");
                throw new InvalidOperationException("Пользователь не найден");
            }

            Console.WriteLine($"✅ Пользователь найден: {user.FirstName} {user.LastName}");

            // Проверяем доступность автомобиля на указанные даты
            var isAvailable = await _carRepository.IsCarAvailableAsync(
                requestDto.CarId, requestDto.StartDate, requestDto.EndDate);

            Console.WriteLine($"🔍 Проверка доступности автомобиля: {isAvailable}");

            if (!isAvailable)
                throw new InvalidOperationException("Автомобиль недоступен на выбранные даты");

            // Создаем бронирование
            var booking = new Domain.Entities.Booking
            {
                CarId = requestDto.CarId,
                UserId = requestDto.UserId,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                TotalPrice = requestDto.TotalPrice,
                Notes = requestDto.Notes,
                Status = Domain.Enums.BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"📝 Создано бронирование: {booking.Id}");

            await _bookingRepository.AddAsync(booking);
            Console.WriteLine("💾 Бронирование добавлено в репозиторий");

            var saved = await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"💾 Сохранение в БД: {saved}");

            if (!saved)
                throw new InvalidOperationException("Не удалось сохранить бронирование в базу данных");

            // Загружаем связанные данные для возврата
            var createdBooking = await _bookingRepository.GetByIdAsync(booking.Id);
            if (createdBooking == null)
            {
                Console.WriteLine("❌ Бронирование не найдено после создания");
                throw new InvalidOperationException("Бронирование не найдено после создания");
            }

            Console.WriteLine($"✅ Бронирование успешно создано и сохранено: ID={createdBooking.Id}");
            
            return _mapper.Map<BookingDto>(createdBooking);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ОШИБКА в BookingService.CreateBookingAsync: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // Остальные методы остаются без изменений...
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
            throw new InvalidOperationException("Автомобиль не найден");

        var days = (requestDto.EndDate - requestDto.StartDate).Days;
        var totalPrice = car.PricePerDay * days;

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