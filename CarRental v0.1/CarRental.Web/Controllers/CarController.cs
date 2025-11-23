using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Car;

namespace CarRental.Web.Controllers;

public class CarController : Controller
{
    private readonly ICarService _carService;

    public CarController(ICarService carService)
    {
        _carService = carService;
    }

    public async Task<IActionResult> Index()
    {
        var cars = await _carService.GetAllCarsAsync();
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
        return View(car);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string brand, string model, decimal? maxPrice)
    {
        // ВАЛИДАЦИЯ ВХОДНЫХ ДАННЫХ
        if (maxPrice < 0)
        {
            ModelState.AddModelError("maxPrice", "Price cannot be negative");
            return View("Index", new List<CarDto>());
        }

        // SANITIZE СТРОКОВЫЕ ПАРАМЕТРЫ
        brand = System.Net.WebUtility.HtmlEncode(brand ?? "");
        model = System.Net.WebUtility.HtmlEncode(model ?? "");
        
        var filter = new CarFilterDto 
        { 
            Brand = brand, 
            Model = model, 
            MaxPrice = maxPrice 
        };
        
        var cars = await _carService.GetCarsByFilterAsync(filter);
        return View("Index", cars);
    }
}