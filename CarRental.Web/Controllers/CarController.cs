using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Car;
using CarRental.Web.ViewModels.Car;
using CarRental.Domain.Entities;
using CarRental.BLL.DTOs.Booking;

namespace CarRental.Web.Controllers;

public class CarController : Controller
{
    private readonly ICarService _carService;
    private readonly IBookingService _bookingService;
    private readonly UserManager<User> _userManager;

    public CarController(ICarService carService, IBookingService bookingService, UserManager<User> userManager)
    {
        _carService = carService;
        _bookingService = bookingService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var cars = await _carService.GetAllCarsAsync();
        await LoadFilterData();
        ViewBag.HasActiveFilters = false;
        ViewBag.CurrentBrand = "";
        ViewBag.CurrentModel = "";
        ViewBag.CurrentMaxPrice = "";
        return View(cars);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid car ID");
        }

        var car = await _carService.GetCarByIdAsync(id);
        if (car == null)
        {
            return NotFound();
        }

        BookCarViewModel? bookingModel = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            bookingModel = new BookCarViewModel { 
                CarId = id,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(4)
            };
        }
        
        // Используем ValueTuple вместо Tuple
        var model = (Car: car, BookingModel: bookingModel);
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string brand, string model, decimal? maxPrice)
    {
        Console.WriteLine($"🔍 SEARCH REQUEST: Brand='{brand}', Model='{model}', MaxPrice={maxPrice}");

        try
        {
            // Загружаем данные для фильтров ДО фильтрации
            await LoadFilterData();
            
            // Устанавливаем текущие значения фильтров
            ViewBag.CurrentBrand = brand ?? "";
            ViewBag.CurrentModel = model ?? "";
            ViewBag.CurrentMaxPrice = maxPrice?.ToString() ?? "";
            ViewBag.HasActiveFilters = !string.IsNullOrEmpty(brand) || !string.IsNullOrEmpty(model) || maxPrice.HasValue;
            
            // Применяем фильтрацию
            var filter = new CarFilterDto 
            { 
                Brand = brand ?? "", 
                Model = model ?? "", 
                MaxPrice = maxPrice 
            };
            
            var cars = await _carService.GetCarsByFilterAsync(filter);
            Console.WriteLine($"📊 SEARCH RESULTS: {cars.Count()} cars found");
            
            return View("Index", cars);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 SEARCH ERROR: {ex.Message}");
            TempData["ErrorMessage"] = "Ошибка при выполнении поиска";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookCar(BookCarViewModel model)
    {
        try
        {
            Console.WriteLine("=== 🚗 CAR BOOKING START ===");
            Console.WriteLine($"CarId: {model.CarId}, Start: {model.StartDate}, End: {model.EndDate}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ BOOKING: ModelState invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                TempData["ErrorMessage"] = "Пожалуйста, исправьте ошибки в форме.";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            var car = await _carService.GetCarByIdAsync(model.CarId);
            if (car == null || !car.IsAvailable)
            {
                Console.WriteLine($"❌ Car not available: CarId={model.CarId}, Available={car?.IsAvailable}");
                TempData["ErrorMessage"] = "Автомобиль недоступен для бронирования";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            // Получаем ID пользователя
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ User not found");
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            // Расчет стоимости
            var days = (model.EndDate - model.StartDate).Days;
            var basePrice = car.PricePerDay * days;
            var deliveryPrice = model.NeedDelivery ? GetDeliveryPrice(model.DeliveryLocation) : 0;
            var unlimitedPrice = model.UnlimitedMileage ? 2000 * days : 0;
            var totalPrice = basePrice + deliveryPrice + unlimitedPrice;

            Console.WriteLine($"💰 Price calculation: Days={days}, Base={basePrice}, Delivery={deliveryPrice}, Unlimited={unlimitedPrice}, Total={totalPrice}");

            var bookingDto = new BookingRequestDto
            {
                CarId = model.CarId,
                UserId = Guid.Parse(userId),
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Notes = $"Доставка: {(model.NeedDelivery ? model.DeliveryLocation?.ToString() : "Нет")}, " +
                    $"Безлимит: {(model.UnlimitedMileage ? "Да" : "Нет")}, " +
                    $"Итоговая стоимость: {totalPrice}₽",
                TotalPrice = totalPrice
            };

            var result = await _bookingService.CreateBookingAsync(bookingDto);
            
            if (result != null)
            {
                Console.WriteLine($"✅ BOOKING SUCCESS: {result.Id}");
                TempData["SuccessMessage"] = $"Автомобиль успешно забронирован! Стоимость: {totalPrice}₽";
                return RedirectToAction("Bookings", "Profile");
            }
            else
            {
                Console.WriteLine($"❌ BOOKING FAILED");
                TempData["ErrorMessage"] = "Ошибка при создании бронирования";
                return RedirectToAction("Details", new { id = model.CarId });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 BOOKING ERROR: {ex}");
            TempData["ErrorMessage"] = $"Произошла ошибка при бронировании: {ex.Message}";
            return RedirectToAction("Details", new { id = model.CarId });
        }
    }

        private async Task LoadFilterData()
        {
            try
            {
                var brands = await _carService.GetUniqueBrandsAsync();
                var models = await _carService.GetUniqueModelsAsync();
                
                ViewBag.Brands = brands ?? new List<string>();
                ViewBag.Models = models ?? new List<string>();
                
                Console.WriteLine($"📊 FILTER DATA: {brands?.Count() ?? 0} brands, {models?.Count() ?? 0} models loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR LOADING FILTER DATA: {ex.Message}");
                ViewBag.Brands = new List<string>();
                ViewBag.Models = new List<string>();
            }
        }

    private decimal GetDeliveryPrice(DeliveryLocation? location)
    {
        return location switch
        {
            DeliveryLocation.Sheremetyevo => 4500,
            DeliveryLocation.Vnukovo => 3500,
            DeliveryLocation.Domodedovo => 6000,
            DeliveryLocation.Moscow => 0, // Индивидуально (обговаривается с менеджером)
            _ => 0
        };
    }

    private void SendBookingNotifications(CarRental.BLL.DTOs.Booking.BookingDto booking)
    {
        // Логирование для администратора
        Console.WriteLine($"\n📋 ===== НОВОЕ БРОНИРОВАНИЕ ===== 📋");
        Console.WriteLine($"🔹 ID: {booking.Id}");
        Console.WriteLine($"👤 Клиент: {booking.User?.FirstName} {booking.User?.LastName}");
        Console.WriteLine($"📧 Email: {booking.User?.Email}");
        Console.WriteLine($"📞 Телефон: {booking.User?.PhoneNumber}");
        Console.WriteLine($"🚗 Автомобиль: {booking.Car?.Brand} {booking.Car?.Model}");
        Console.WriteLine($"📅 Период: {booking.StartDate:dd.MM.yyyy} - {booking.EndDate:dd.MM.yyyy}");
        Console.WriteLine($"💰 Стоимость: {booking.TotalPrice}₽");
        Console.WriteLine($"📝 Примечания: {booking.Notes}");
        Console.WriteLine($"⏰ Создано: {booking.CreatedAt:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"📋 =============================== 📋\n");
    }
}