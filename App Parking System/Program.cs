using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using App_Parking_System.Data;
using App_Parking_System.Repositories.Contract;
using App_Parking_System.Repositories;
using App_Parking_System.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ParkingSystemDb"));
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
builder.Services.AddScoped<IParkingSettingRepository, ParkingSettingRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddControllersWithViews();
builder.Services.Configure<ParkingSettings>(builder.Configuration.GetSection("ParkingSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Parking}/{action=Index}/{id?}");

app.Run();