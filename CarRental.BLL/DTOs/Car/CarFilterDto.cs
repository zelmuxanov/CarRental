namespace CarRental.BLL.DTOs.Car;

public class CarFilterDto
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? Brands { get; set; }
    public List<string>? Models { get; set; }
    public List<Domain.Enums.TransmissionType>? TransmissionTypes { get; set; }
    public List<Domain.Enums.FuelType>? FuelTypes { get; set; }
    public List<Domain.Enums.CarClass>? Classes { get; set; }
    public int? MinSeats { get; set; }
    public int? MaxSeats { get; set; }
}