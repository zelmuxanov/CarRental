using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CarRental.Web.ViewModels.Profile;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Entities;

namespace CarRental.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IBookingService _bookingService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IUserService userService,
        IBookingService bookingService,
        UserManager<User> userManager,
        ILogger<ProfileController> logger)
    {
        _userService = userService;
        _bookingService = bookingService;
        _userManager = userManager;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(_userManager.GetUserId(User) ?? throw new UnauthorizedAccessException());

    public async Task<IActionResult> Index()
    {
        try
        {
            var userProfile = await _userService.GetUserByIdAsync(UserId);
            if (userProfile == null) return NotFound();

            var userBookings = await _bookingService.GetUserBookingsAsync(UserId);
            var upcomingBookings = userBookings
                .Where(b => b.StartDate >= DateTime.UtcNow && b.Status == Domain.Enums.BookingStatus.Confirmed)
                .OrderBy(b => b.StartDate)
                .Take(3);

            var model = new ProfileViewModel
            {
                UserProfile = userProfile,
                VerificationInfo = await _userService.GetUserVerificationInfoAsync(UserId),
                UpcomingBookings = upcomingBookings,
                TotalBookingsCount = userBookings.Count()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке профиля пользователя {UserId}", UserId);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке профиля";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: /Profile/Edit
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        try
        {
            var userProfile = await _userService.GetUserByIdAsync(UserId);
            if (userProfile == null) return NotFound();

            var model = new EditProfileViewModel
            {
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                PhoneNumber = userProfile.PhoneNumber ?? string.Empty
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке формы редактирования профиля {UserId}", UserId);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке формы";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userProfileDto = new UserProfileDto
            {
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                PhoneNumber = model.PhoneNumber ?? string.Empty
            };

            var result = await _userService.UpdateUserProfileAsync(UserId, userProfileDto);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлен";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Ошибка при обновлении профиля");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении профиля пользователя {UserId}", UserId);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении профиля");
            return View(model);
        }
    }

    // GET: /Profile/ChangePassword
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: /Profile/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _userService.ChangePasswordAsync(UserId, model.CurrentPassword, model.NewPassword);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменен";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при смене пароля пользователя {UserId}", UserId);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при смене пароля");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Bookings()
    {
        try
        {
            var bookings = await _bookingService.GetUserBookingsAsync(UserId);
            return View(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке бронирований пользователя {UserId}", UserId);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке бронирований";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Documents()
    {
        try
        {
            var documents = await _userService.GetUserDocumentsAsync(UserId);
            var model = new DocumentsViewModel
            {
                Documents = documents,
                CanUploadDocuments = documents.VerificationStatus != "Verified"
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке документов пользователя {UserId}", UserId);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке документов";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult UploadDocuments()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocuments(DocumentUploadViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            if (model.DriverLicenseExpiry.HasValue && model.DriverLicenseExpiry.Value <= DateTime.UtcNow)
            {
                ModelState.AddModelError("DriverLicenseExpiry", "Срок действия должен быть в будущем");
                return View(model);
            }

            var documentsDto = new UserDocumentsDto
            {
                DriverLicenseNumber = model.DriverLicenseNumber,
                DriverLicenseExpiry = model.DriverLicenseExpiry
            };

            var result = await _userService.UpdateUserDocumentsAsync(UserId, documentsDto);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Документы успешно загружены и отправлены на проверку";
                return RedirectToAction(nameof(Documents));
            }

            ModelState.AddModelError(string.Empty, "Ошибка при сохранении документов");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке документов пользователя {UserId}", UserId);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при загрузке документов");
            return View(model);
        }
    }
}