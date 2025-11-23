using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Car;

public class CarDto
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public CarClass Class { get; set; }
    public TransmissionType Transmission { get; set; }
    public FuelType FuelType { get; set; }
    public int Seats { get; set; }
    public double EngineCapacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}