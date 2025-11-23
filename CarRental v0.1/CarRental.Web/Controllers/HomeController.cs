using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace CarRental.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICarService _carService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ICarService carService, ILogger<HomeController> logger)
    {
        _carService = carService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Загрузка главной страницы");
            var cars = await _carService.GetAvailableCarsAsync();
            return View(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке автомобилей на главной странице");
            ViewBag.ErrorMessage = "Не удалось загрузить список автомобилей";
            return View("Error");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var errorId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        var exception = exceptionHandlerPathFeature?.Error;
        if (exception != null)
        {
            _logger.LogError(exception, "Произошла ошибка: {ErrorId}", errorId);

            ViewBag.ErrorId = errorId;
            ViewBag.ErrorMessage = exception.Message;
            ViewBag.Path = exceptionHandlerPathFeature?.Path;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}