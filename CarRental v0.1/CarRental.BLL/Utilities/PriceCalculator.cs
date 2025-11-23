namespace CarRental.BLL.Utilities;

public class PriceCalculator
{
    public decimal CalculatePrice(decimal pricePerDay, DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).Days;
        if (days < 1) days = 1; // Минимум 1 день

        return pricePerDay * days;
    }
}
