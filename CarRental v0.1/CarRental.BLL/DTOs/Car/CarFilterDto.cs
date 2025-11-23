using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Car;

public class CarFilterDto
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal? MaxPrice { get; set; }
    public CarClass? Class { get; set; }
    public TransmissionType? Transmission { get; set; }
    public FuelType? FuelType { get; set; }
}
