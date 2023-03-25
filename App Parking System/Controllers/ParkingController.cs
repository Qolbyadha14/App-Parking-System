using App_Parking_System.Data;
using App_Parking_System.Dto;
using App_Parking_System.Helpers;
using App_Parking_System.Models;
using App_Parking_System.Repositories;
using App_Parking_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;

namespace App_Parking_System.Controllers
{
    public class ParkingController : Controller
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IParkingLotRepository _parkingLotRepository;
        private readonly ApplicationDbContext _parkingDbContext;
        private readonly ILogger<ParkingController> _logger;
        private readonly ParkingSettings _parkingSettings;

        public ParkingController(IVehicleRepository vehicleRepository, IParkingLotRepository parkingLotRepository, ApplicationDbContext parkingDbContext, ILogger<ParkingController> logger, IOptions<ParkingSettings> parkingSettings)
        {
            _vehicleRepository = vehicleRepository;
            _parkingLotRepository = parkingLotRepository;
            _parkingDbContext = parkingDbContext;
            _logger = logger;
            _parkingSettings = parkingSettings.Value;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckIn(CheckinRequest request)
        {
            var r = request;
            var vehicle = ObjectHelpers.Convert<CheckinRequest, Vehicle>(request);
            if (ModelState.IsValid)
            {
                var existingVehicle = _parkingDbContext.Vehicles.FirstOrDefault(v => v.PoliceNumber == vehicle.PoliceNumber && v.CheckOutTime == null);
                if (existingVehicle != null)
                {
                    TempData["ErrorMessage"] = "Gagal check-in: Kendaraan dengan nomor polisi yang sama sudah check-in.";
                    return RedirectToAction(nameof(Index));
                }

                var availableLots = Enumerable.Range(1, _parkingSettings.MaxLots).Except(_parkingDbContext.Vehicles.Where(v => v.CheckOutTime == null).Select(v => v.LotNumber)).ToList();
                if (availableLots.Any())
                {
                    vehicle.LotNumber = availableLots.Min();
                    _parkingDbContext.Vehicles.Add(vehicle);
                    _parkingDbContext.SaveChanges();
                    TempData["SuccessMessage"] = $"Check-in berhasil! Kendaraan dialokasikan ke lot {vehicle.LotNumber}.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Gagal check-in: Semua lot parkir penuh.";
                    return RedirectToAction(nameof(Index));
                }
            }
           
            var errors = new StringBuilder();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    errors.AppendLine(error.ErrorMessage);
                }
            }

            TempData["ErrorMessage"] = $"Gagal check-in: {errors}";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public IActionResult CheckOut(CheckOutRequest vehicle)
        {
            if (ModelState.IsValid)
            {
                var existingVehicle = _parkingDbContext.Vehicles.FirstOrDefault(v => v.PoliceNumber == vehicle.PoliceNumber && v.CheckOutTime == null);
                if (existingVehicle != null)
                {
                    existingVehicle.CheckOutTime = DateTime.Now;
                    _parkingDbContext.SaveChanges();
                    TempData["SuccessMessage"] = $"Check-out berhasil! Slot number {existingVehicle.LotNumber} is free";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Gagal check-out: Kendaraan dengan nomor polisi yang dimasukkan tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var errors = new StringBuilder();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    errors.AppendLine(error.ErrorMessage);
                }
            }

            TempData["ErrorMessage"] = $"Gagal check-in: {errors}";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reports()
        {
            var vehicleGroups_type = _parkingDbContext.Vehicles
            .Where(v => v.CheckOutTime == null)
            .GroupBy(v => v.Type)
            .Select(g => new VehicleGroup
            {
                Key = g.Key.ToString(),
                Count = g.Count()
            })
            .ToList();

            var vehicleGroups_police_number = _parkingDbContext.Vehicles
                .Where(v => v.CheckOutTime == null)
                .GroupBy(v => ExtractNumberFromPoliceNumber(v.PoliceNumber) % 2 == 0 ? "Even" : "Odd")
                .Select(g => new VehicleGroup
                {
                    Key = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var vehicleGroups_color = _parkingDbContext.Vehicles
                .Where(v => v.CheckOutTime == null)
                .GroupBy(v => v.Color)
                .Select(g => new VehicleGroup
                {
                    Key = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            var viewModel = new ReportsViewModel
            {
                TotalLots = _parkingSettings.MaxLots,
                AvailableLots = _parkingSettings.MaxLots - _parkingDbContext.Vehicles.Count(v => v.CheckOutTime == null),
                VehiclesByColors = vehicleGroups_color,
                VehiclesByType = vehicleGroups_type,
                VehiclesByPoliceNumber = vehicleGroups_police_number,
                Vehicles = _parkingDbContext.Vehicles.Where(v => v.CheckOutTime == null).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateTestData()
        {
            var random = new Random();
            var vehicleTypes = Enum.GetValues(typeof(VehicleType)).Cast<VehicleType>().ToArray();

            for (int i = 0; i <= 5; i++)
            {
                var vehicle = new Vehicle()
                {
                    CheckInTime = DateTime.Now,
                    LotNumber = i + 1,
                    Color = "Blue",
                    PoliceNumber = $"B-270{i}-XXX",
                    Type = vehicleTypes[random.Next(vehicleTypes.Length)],
                };

                _parkingDbContext.Vehicles.Add(vehicle);
                _parkingDbContext.SaveChanges();
            }
            TempData["ErrorMessage"] = "Create Test Data Berhasil";
            return RedirectToAction(nameof(Index));
        }

        private int ExtractNumberFromPoliceNumber(string policeNumber)
        {
            var numberString = new string(policeNumber.Where(char.IsDigit).ToArray());
            return int.Parse(numberString);
        }
    }
}
