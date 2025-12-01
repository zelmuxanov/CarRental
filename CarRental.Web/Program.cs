using Microsoft.EntityFrameworkCore;
using CarRental.DAL.Data;
using CarRental.DAL.Repositories;
using CarRental.BLL.Services;
using CarRental.BLL.Utilities;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using CarRental.BLL.DTOs.User;
using CarRental.BLL.Validators;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5123");
builder.Services.AddHttpsRedirection(options => { options.HttpsPort = null; });

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CarRental.DAL"))); 

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Utilities
builder.Services.AddScoped<PriceCalculator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(CarRental.BLL.Profiles.CarProfile));

//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();

var app = builder.Build();

// 🔥 SEED DATA (ПЕРЕД настройкой pipeline)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Применяем миграции
        context.Database.Migrate();
        
        // Инициализируем данные
        DbInitializer.Initialize(context);
        
        Console.WriteLine("✅ Database migrated and seeded successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var staticFileOptions = new StaticFileOptions
{
    ServeUnknownFileTypes = false,
    DefaultContentType = "text/plain",
    OnPrepareResponse = ctx =>
    {
        // Кэширование статических файлов
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
    }
};

app.Use(async (context, next) =>
{
    context.Response.Headers.ContentType = "text/html; charset=utf-8";
    await next();
});

// ДОБАВИТЬ ДЛЯ ДИАГНОСТИКИ (в Development):
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// VFHIHENS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "profile",
    pattern: "Profile/{action=Index}/{id?}",
    defaults: new { controller = "Profile" });
app.MapControllerRoute(
    name: "booking",
    pattern: "Booking/{action}/{id?}",
    defaults: new { controller = "Booking" });

//app.MapRazorPages();

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources.SelectMany(es => es.Endpoints);
    return Results.Json(endpoints.Select(e => new {
        DisplayName = e.DisplayName,
        RoutePattern = (e as RouteEndpoint)?.RoutePattern.RawText,
        HttpMethods = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods
    }));
});

app.Run();