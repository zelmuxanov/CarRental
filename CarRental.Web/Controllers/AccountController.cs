using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CarRental.Web.ViewModels.Account;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace CarRental.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _logger = logger;
    }

    // ✅ GET метод для отображения формы регистрации
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // ✅ POST метод для обработки регистрации
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            Console.WriteLine("=== 🔍 REGISTER DIAGNOSTICS START ===");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"FirstName: {model.FirstName}");
            Console.WriteLine($"LastName: {model.LastName}");
            Console.WriteLine($"BirthDate: {model.BirthDate}");
            Console.WriteLine($"DrivingExperience: {model.DrivingExperience}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalid");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($" - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            // ✅ ЯВНАЯ ВАЛИДАЦИЯ КРИТИЧЕСКИХ ПОЛЕЙ
            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError("BirthDate", "Дата рождения обязательна");
                return View(model);
            }

            if (!model.DrivingExperience.HasValue)
            {
                ModelState.AddModelError("DrivingExperience", "Стаж вождения обязателен");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль обязателен");
                return View(model);
            }

            // ✅ СОЗДАЕМ ПОЛЬЗОВАТЕЛЯ НАПРЯМУЮ ЧЕРЕЗ UserManager
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                // ✅ ИСПРАВЛЕНИЕ: Используем только дату без времени и явно указываем UTC
                BirthDate = new DateTime(model.BirthDate.Value.Year, model.BirthDate.Value.Month, model.BirthDate.Value.Day, 0, 0, 0, DateTimeKind.Utc),
                DrivingExperience = model.DrivingExperience.Value,
                Status = Domain.Enums.UserStatus.Pending,
                RegistrationDate = DateTime.UtcNow // ✅ Уже в UTC
            };

            Console.WriteLine("📝 Creating user with UserManager.CreateAsync...");
            
            // ✅ СОЗДАЕМ ПОЛЬЗОВАТЕЛЯ С ПАРОЛЕМ
            var result = await _userManager.CreateAsync(user, model.Password);
            
            Console.WriteLine($"📊 UserManager.CreateAsync result: Succeeded={result.Succeeded}");

            if (result.Succeeded)
            {
                Console.WriteLine("✅ UserManager: пользователь успешно создан");

                // ✅ НЕМЕДЛЕННО ПРОВЕРЯЕМ СОХРАНЕНИЕ В БД
                var savedUser = await _userManager.FindByEmailAsync(model.Email);
                Console.WriteLine($"🔍 User after creation: {savedUser != null}");

                if (savedUser != null)
                {
                    Console.WriteLine($"📋 User details: ID={savedUser.Id}, Email={savedUser.Email}");

                    // ✅ СЧИТАЕМ ВСЕХ ПОЛЬЗОВАТЕЛЕЙ В БАЗЕ
                    var allUsers = _userManager.Users.ToList();
                    Console.WriteLine($"📊 TOTAL USERS IN DATABASE: {allUsers.Count}");
                    
                    foreach (var u in allUsers)
                    {
                        Console.WriteLine($"👤 User: {u.UserName}, Email: {u.Email}, ID: {u.Id}");
                    }

                    // ✅ ВХОДИМ В СИСТЕМУ
                    await _signInManager.SignInAsync(savedUser, isPersistent: false);
                    Console.WriteLine("🎉 REGISTRATION COMPLETE - redirecting to home");
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("❌ USER NOT FOUND AFTER CREATION!");
                    ModelState.AddModelError(string.Empty, "Пользователь не был сохранен в базу данных");
                }
            }
            else
            {
                Console.WriteLine("❌ USER MANAGER ERRORS:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($" - {error.Code}: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 REGISTRATION EXCEPTION: {ex}");
            _logger.LogError(ex, "Ошибка при регистрации пользователя {Email}", model.Email);
            ModelState.AddModelError(string.Empty, $"Ошибка регистрации: {ex.Message}");
            return View(model);
        }
    }

    // ✅ GET метод для отображения формы входа
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ✅ POST метод для обработки входа
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;

            Console.WriteLine("=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"Password: [HIDDEN]");
            Console.WriteLine($"Password length: {model.Password?.Length ?? 0}");
            Console.WriteLine($"RememberMe: {model.RememberMe}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid");
                return View(model);
            }

            // ✅ ЯВНАЯ ВАЛИДАЦИЯ ПАРОЛЯ
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль обязателен");
                return View(model);
            }

            // ✅ ПОИСК ПОЛЬЗОВАТЕЛЯ
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"❌ USER NOT FOUND: {model.Email}");
                ModelState.AddModelError(string.Empty, "Пользователь с таким email не найден");
                return View(model);
            }

            Console.WriteLine($"✅ USER FOUND: ID={user.Id}, UserName={user.UserName}");

            // ✅ ПРОВЕРКА ПАРОЛЯ
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            Console.WriteLine($"🔐 PASSWORD VALID: {passwordValid}");

            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Неверный пароль");
                return View(model);
            }

            // ✅ ВХОД В СИСТЕМУ
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            Console.WriteLine($"Login result: Succeeded={result.Succeeded}");

            if (result.Succeeded)
            {
                Console.WriteLine($"✅ LOGIN SUCCESS for {model.Email}");
                _logger.LogInformation("Пользователь {Email} вошел в систему", model.Email);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                Console.WriteLine($"❌ LOGIN FAILED for {model.Email}");
                ModelState.AddModelError(string.Empty, "Ошибка входа");
            }

            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 LOGIN ERROR: {ex.Message}");
            _logger.LogError(ex, "Ошибка при входе пользователя {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при входе в систему");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            Console.WriteLine("🔄 GET LOGOUT CALLED - User: " + User?.Identity?.Name);
            
            await _signInManager.SignOutAsync();
            
            Console.WriteLine("✅ GET LOGOUT COMPLETED - User signed out");
            
            // ✅ ДОБАВЛЯЕМ ЗАЩИТНЫЕ ЗАГОЛОВКИ
            Response.Headers["Cache-Control"] = "no-cache, no-store";
            Response.Headers["Pragma"] = "no-cache";
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 GET LOGOUT ERROR: {ex.Message}");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Пользователь вышел из системы");
        return RedirectToAction("Index", "Home");
    }

    // ✅ ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ТЕСТИРОВАНИЯ
    [HttpGet]
    public IActionResult RegisterSimple()
    {
        return View();
    }

    [HttpGet]
    public IActionResult TestModelBinding()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        else
            return RedirectToAction("Index", "Home");
    }

    // 🔐 МЕТОДЫ ВОССТАНОВЛЕНИЯ ПАРОЛЯ

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel()); // ✅ Создаем новую модель
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model); // ✅ Возвращаем ту же модель

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Не показываем, что пользователь не существует (безопасность)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Генерируем токен сброса пароля
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // В реальном приложении здесь отправляем email с ссылкой
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = model.Email;
            
            _logger.LogInformation("Reset token for {Email}: {Token}", model.Email, token);
            
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе сброса пароля для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при запросе сброса пароля");
            return View(model); // ✅ Возвращаем модель с ошибками
        }
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError(string.Empty, "Неверная ссылка для сброса пароля");
        }
        
        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Не показываем, что пользователь не существует
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сбросе пароля для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при сбросе пароля");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}