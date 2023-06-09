using App_Parking_System.Controllers;
using App_Parking_System.Data;
using App_Parking_System.Models;
using App_Parking_System.Repositories;
using App_Parking_System.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace AppParkingSystem.Tests
{
    public class ParkingControllerTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IParkingLotRepository> _parkingLotRepositoryMock;
        private readonly Mock<IParkingSettingRepository> _parkingSettingRepositoryMock;
        private readonly Mock<IReportRepository> _reportRepositoryMock;
        private readonly ILoggerFactory _loggerFactory;

        public ParkingControllerTests()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _parkingLotRepositoryMock = new Mock<IParkingLotRepository>();
            _parkingSettingRepositoryMock = new Mock<IParkingSettingRepository>();
            _reportRepositoryMock = new Mock<IReportRepository>();
            _loggerFactory = new LoggerFactory();
        }

        [Fact]
        public async void Test_CheckIn_Success()
        {
            // Arrange
            _parkingSettingRepositoryMock
                .Setup(repo => repo.GetParkingSettings(It.IsAny<string>()))
                .ReturnsAsync(new App_Parking_System.Models.ParkingSettings { Value = "5" });

            _vehicleRepositoryMock
                .Setup(repo => repo.GetExistingVehicleByPoliceNumber(It.IsAny<string>()))
                .ReturnsAsync((App_Parking_System.Models.Vehicle)null);

            _vehicleRepositoryMock
                .Setup(repo => repo.GetExistingVehicle())
                .ReturnsAsync(new System.Collections.Generic.List<App_Parking_System.Models.Vehicle>());
            
            // Setup Untuk Sqlite
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
           .UseSqlite($"DataSource=:memory:");
            var dbContextMock = new ApplicationDbContext(optionsBuilder.Options);
            await dbContextMock.Database.OpenConnectionAsync(); // Buka koneksi ke database SQLite In-Memory
            await dbContextMock.Database.EnsureCreatedAsync(); // Buat skema database
            var httpContext = new DefaultHttpContext();

            // Setup Response temp data array karena ada balikan message di controllers
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            var tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
            tempDataFactoryMock.Setup(s => s.GetTempData(It.IsAny<HttpContext>())).Returns(tempData);


            // setup harus sama constructor controller utama
            var controller = new ParkingController(
                _vehicleRepositoryMock.Object,
                _parkingLotRepositoryMock.Object,
                dbContextMock,
                _loggerFactory,
                _parkingSettingRepositoryMock.Object,
                _reportRepositoryMock.Object,
                tempDataFactoryMock.Object
            );

            controller.ControllerContext.HttpContext = httpContext;
            controller.TempData = tempData;

            var request = new CheckInViewModel
            {
                PoliceNumber = "B-1234-XYZ",
                Color = "Blue",
                Type = App_Parking_System.Models.VehicleType.Car,

            };

            // Act
            var result = await controller.CheckIn(request);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal($"Check-in berhasil! Kendaraan dialokasikan ke lot {1}.", controller.TempData["SuccessMessage"]);
        }

    }
}