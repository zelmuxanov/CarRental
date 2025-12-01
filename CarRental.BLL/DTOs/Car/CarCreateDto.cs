namespace CarRental.BLL.DTOs.Car;

public class CarCreateDto
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public Domain.Enums.CarClass Class { get; set; }
    public Domain.Enums.TransmissionType Transmission { get; set; }
    public Domain.Enums.FuelType FuelType { get; set; }
    public int Seats { get; set; }
    public double EngineCapacity { get; set; }
    public double LimitPerDay { get; set; }
}