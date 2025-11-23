using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Document : BaseEntity
{
    public Guid UserId { get; set; }
    public DocumentType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }

    // Навигационные свойства
    public virtual User User { get; set; } = null!;
}
