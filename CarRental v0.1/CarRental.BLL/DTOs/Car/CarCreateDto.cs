using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.DTOs.Car;

public class CarCreateDto
{
    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    public string Color { get; set; } = string.Empty;

    [Required]
    public string VIN { get; set; } = string.Empty;

    [Required]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [Range(0, 10000)]
    public decimal PricePerDay { get; set; }

    [Required]
    [Range(0, 1000)]
    public decimal PricePerHour { get; set; }

    public string? ImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public Domain.Enums.CarClass Class { get; set; }
    public Domain.Enums.TransmissionType Transmission { get; set; }
    public Domain.Enums.FuelType FuelType { get; set; }
    public int Seats { get; set; }
    public double EngineCapacity { get; set; }
}
