using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

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
            
            // Загружаем данные для фильтров
            var brands = await _carService.GetUniqueBrandsAsync();
            var models = await _carService.GetUniqueModelsAsync();
            
            ViewBag.Brands = brands;
            ViewBag.Models = models;
            
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
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}