using App_Parking_System.Config;
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
        private readonly IParkingSettingRepository _parkingSettingRepository;
        private readonly ApplicationDbContext _parkingDbContext;
        private readonly ILogger<ParkingController> _logger;
        private readonly ParkingSettings _parkingSettings;

        public ParkingController(IVehicleRepository vehicleRepository, IParkingLotRepository parkingLotRepository, ApplicationDbContext parkingDbContext, ILogger<ParkingController> logger, IParkingSettingRepository parkingSettingRepository)
        {
            _vehicleRepository = vehicleRepository;
            _parkingLotRepository = parkingLotRepository;
            _parkingDbContext = parkingDbContext;
            _logger = logger;
            _parkingSettingRepository = parkingSettingRepository;


        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(CheckinRequest request)
        {
            var vehicle = ObjectHelpers.Convert<CheckinRequest, Vehicle>(request);
            var checkMaxLot = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_MAXLOT);
            if (checkMaxLot == null)
            {
                TempData["ErrorMessage"] = "Gagal check-in: Parking Max Setting Belum Di Setup.";
                return RedirectToAction(nameof(Setting));
            }

            if (!ModelState.IsValid)
            {
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

            // Todo  : Ganti Pake Repository
            var existingVehicle = _parkingDbContext.Vehicles.FirstOrDefault(v => v.PoliceNumber == vehicle.PoliceNumber && v.CheckOutTime == null);
            if (existingVehicle != null)
            {
                TempData["ErrorMessage"] = "Gagal check-in: Kendaraan dengan nomor polisi yang sama sudah check-in.";
                return RedirectToAction(nameof(Index));
            }

            // Todo  : Ganti Pake Repository
            var availableLots = Enumerable.Range(1, Convert.ToInt16(checkMaxLot.Value)).Except(_parkingDbContext.Vehicles.Where(v => v.CheckOutTime == null).Select(v => v.LotNumber)).ToList();

            if (availableLots.Any())
            {
                // Todo  : Ganti Pake Repository
                vehicle.LotNumber = availableLots.Min();
                vehicle.CheckInTime = DateTime.Now;
                _parkingDbContext.Vehicles.Add(vehicle);
                _parkingDbContext.SaveChanges();
                TempData["SuccessMessage"] = $"Check-in berhasil! Kendaraan dialokasikan ke lot {vehicle.LotNumber}.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Gagal check-in: Semua lot parkir penuh.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CheckOut(CheckOutRequest vehicle)
        {
            if (!ModelState.IsValid)
            {
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

            // Todo  : Ganti Pake Repository
            var existingVehicle = _parkingDbContext.Vehicles.FirstOrDefault(v => v.PoliceNumber == vehicle.PoliceNumber && v.CheckOutTime == null);
            if (existingVehicle != null)
            {
                // Todo  : Ganti Pake Repository
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

        public async Task<IActionResult> Reports()
        {
            var checkMaxLot = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_MAXLOT);

            // Todo  : Ganti Pake Repository
            var vehicleGroups_type = _parkingDbContext.Vehicles
            .Where(v => v.CheckOutTime == null)
            .GroupBy(v => v.Type)
            .Select(g => new VehicleGroup
            {
                Key = g.Key.ToString(),
                Count = g.Count()
            })
            .ToList();

            // Todo  : Ganti Pake Repository
            var vehicleGroups_police_number = _parkingDbContext.Vehicles
                .Where(v => v.CheckOutTime == null)
                .GroupBy(v => ExtractNumberFromPoliceNumber(v.PoliceNumber) % 2 == 0 ? "Even" : "Odd")
                .Select(g => new VehicleGroup
                {
                    Key = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Todo  : Ganti Pake Repository
            var vehicleGroups_color = _parkingDbContext.Vehicles
                .Where(v => v.CheckOutTime == null)
                .GroupBy(v => v.Color)
                .Select(g => new VehicleGroup
                {
                    Key = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            // Todo  : Ganti Pake Repository
            var viewModel = new ReportsViewModel
            {
                TotalLots = checkMaxLot == null ? 0 : Convert.ToInt16(checkMaxLot.Value),
                AvailableLots = checkMaxLot == null ? 0 : Convert.ToInt16(checkMaxLot.Value) - _parkingDbContext.Vehicles.Count(v => v.CheckOutTime == null),
                VehiclesByColors = vehicleGroups_color,
                VehiclesByType = vehicleGroups_type,
                VehiclesByPoliceNumber = vehicleGroups_police_number,
                Vehicles = _parkingDbContext.Vehicles.Where(v => v.CheckOutTime == null).ToList()
            };

            return View(viewModel);
        }


        public async Task<IActionResult> Setting()
        {
            var checkMaxLot = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_MAXLOT);
            var checkPrice = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_PRICE);
            var checkHours = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_HOURS);

            var viewModel = new ParkingSettingViewModel
            {
                MaxLots = checkMaxLot == null ? 0 : Convert.ToInt16(checkMaxLot.Value),
                Price = checkPrice == null ? 0 : Convert.ToInt16(checkPrice.Value),
                Hours = checkHours == null ? 0 : Convert.ToInt16(checkHours.Value)
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SettingUpdate(ParkingSettingRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.AppendLine(error.ErrorMessage);
                    }
                }

                TempData["ErrorMessage"] = $"Gagal Update Setting : {errors}";
                return RedirectToAction(nameof(Setting));
            }

            var settingCodes = new[] { AppConstans.PARKING_SETTING_MAXLOT, AppConstans.PARKING_SETTING_PRICE, AppConstans.PARKING_SETTING_HOURS };
            var requestValues = new[] { request.MaxLots.ToString(), request.Price.ToString(), request.Hours.ToString() };

            for (int i = 0; i < settingCodes.Length; i++)
            {
                var setting = await _parkingSettingRepository.GetParkingSettings(settingCodes[i]);
                var newSetting = new ParkingSettings { Code = settingCodes[i], Name = settingCodes[i], Value = requestValues[i] };

                if (setting == null)
                {
                    await _parkingSettingRepository.AddParkingSettings(newSetting);
                }
                else
                {
                    newSetting.Id = setting.Id;
                    await _parkingSettingRepository.UpdateParkingSettings(newSetting);
                }
            }

            return RedirectToAction(nameof(Setting));
        }

        /*[HttpGet]
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
        }*/

        private int ExtractNumberFromPoliceNumber(string policeNumber)
        {
            var numberString = new string(policeNumber.Where(char.IsDigit).ToArray());
            return int.Parse(numberString);
        }
    }
}
