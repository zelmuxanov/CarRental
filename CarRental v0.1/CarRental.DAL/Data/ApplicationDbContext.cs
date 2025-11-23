using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public ApplicationDbContext() { }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Конфигурации
        builder.ApplyConfiguration(new Configurations.UserConfiguration());
        builder.ApplyConfiguration(new Configurations.CarConfiguration());
        builder.ApplyConfiguration(new Configurations.BookingConfiguration());
        builder.ApplyConfiguration(new Configurations.DocumentConfiguration());
    }
}
