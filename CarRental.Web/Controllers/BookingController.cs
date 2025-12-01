using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using CarRental.BLL.DTOs.Booking;

namespace CarRental.Web.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingService bookingService,
        UserManager<User> userManager,
        ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return Json(new { success = false, message = "Бронирование не найдено" });

            // Проверяем, что пользователь отменяет свое бронирование
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
                return Json(new { success = false, message = "Пользователь не найден" });

            var userId = Guid.Parse(userIdString);
            if (booking.UserId != userId)
                return Json(new { success = false, message = "Нет доступа к этому бронированию" });

            // Проверяем, что можно отменить (только Pending или Confirmed)
            if (booking.Status != Domain.Enums.BookingStatus.Pending && 
                booking.Status != Domain.Enums.BookingStatus.Confirmed)
                return Json(new { success = false, message = "Невозможно отменить это бронирование" });

            var result = await _bookingService.CancelBookingAsync(id);
            
            if (result)
            {
                // Отправляем уведомление менеджеру
                SendCancellationNotification(booking);
                
                return Json(new { success = true, message = "Бронирование отправлено на отмену" });
            }
            
            return Json(new { success = false, message = "Ошибка при отмене бронирования" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
            return Json(new { success = false, message = "Произошла ошибка при отмене бронирования" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return Json(new { success = false, message = "Бронирование не найдено" });

            var result = await _bookingService.ConfirmBookingAsync(id);
            
            if (result)
            {
                // Отправляем уведомление пользователю
                SendConfirmationNotification(booking);
                
                return Json(new { success = true, message = "Бронирование подтверждено" });
            }
            
            return Json(new { success = false, message = "Ошибка при подтверждении бронирования" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", id);
            return Json(new { success = false, message = "Произошла ошибка при подтверждении бронирования" });
        }
    }

    private void SendCancellationNotification(CarRental.BLL.DTOs.Booking.BookingDto booking)
    {
        // Логирование для менеджера
        Console.WriteLine($"\n🚫 ===== ОТМЕНА БРОНИРОВАНИЯ ===== 🚫");
        Console.WriteLine($"🔹 ID: {booking.Id}");
        Console.WriteLine($"👤 Клиент: {booking.User?.FirstName} {booking.User?.LastName}");
        Console.WriteLine($"📧 Email: {booking.User?.Email}");
        Console.WriteLine($"📞 Телефон: {booking.User?.PhoneNumber}");
        Console.WriteLine($"🚗 Автомобиль: {booking.Car?.Brand} {booking.Car?.Model}");
        Console.WriteLine($"💰 Бывшая стоимость: {booking.TotalPrice}₽");
        Console.WriteLine($"⏰ Время отмены: {DateTime.Now:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"🚫 =============================== 🚫\n");
    }

    private void SendConfirmationNotification(CarRental.BLL.DTOs.Booking.BookingDto booking)
    {
        // Логирование для пользователя
        Console.WriteLine($"\n✅ ===== БРОНИРОВАНИЕ ПОДТВЕРЖДЕНО ===== ✅");
        Console.WriteLine($"🔹 ID: {booking.Id}");
        Console.WriteLine($"👤 Клиент: {booking.User?.FirstName} {booking.User?.LastName}");
        Console.WriteLine($"🚗 Автомобиль: {booking.Car?.Brand} {booking.Car?.Model}");
        Console.WriteLine($"💰 Стоимость: {booking.TotalPrice}₽");
        Console.WriteLine($"⏰ Время подтверждения: {DateTime.Now:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"✅ ================================== ✅\n");
    }
}