namespace CarRental.Domain.Enums;

public enum UserStatus
{
    Pending,    // Требует активации администратором
    Active,     // Активный
    Blocked     // Заблокирован
}
