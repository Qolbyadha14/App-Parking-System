using App_Parking_System.Config;
using App_Parking_System.Data;
using App_Parking_System.Helpers;
using App_Parking_System.Models;
using App_Parking_System.Repositories;
using App_Parking_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using App_Parking_System.Repositories.Contract;

namespace App_Parking_System.Controllers
{
    public class ParkingController : Controller
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IParkingLotRepository _parkingLotRepository;
        private readonly IParkingSettingRepository _parkingSettingRepository;
        private readonly IReportRepository _reportRepository;
        private readonly ApplicationDbContext _parkingDbContext;
        private readonly ILogger<ParkingController> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public ParkingController(IVehicleRepository vehicleRepository, IParkingLotRepository parkingLotRepository, ApplicationDbContext parkingDbContext, ILoggerFactory loggerFactory, IParkingSettingRepository parkingSettingRepository, IReportRepository reportRepository, ITempDataDictionaryFactory tempDataFactory)
        {
            _vehicleRepository = vehicleRepository;
            _parkingLotRepository = parkingLotRepository;
            _parkingDbContext = parkingDbContext;
            _logger = loggerFactory.CreateLogger<ParkingController>();
            _parkingSettingRepository = parkingSettingRepository;
            _reportRepository = reportRepository;
            _tempDataFactory = tempDataFactory;
        }

        public IActionResult Index()
        {
            return View();
        }


        // action used when the car enters
        [HttpPost]
        [Authorize(Policy = "CustomerServicesPolicy")]
        public async Task<IActionResult> CheckIn(CheckInViewModel request)
        {

            var vehicle = ObjectHelpers.Convert<CheckInViewModel, Vehicle>(request);
            var checkMaxLot = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_MAXLOT);
            var tempData = _tempDataFactory.GetTempData(HttpContext);

            //Check If Max Lot Parking Null
            if (checkMaxLot == null)
            {
                TempData["ErrorMessage"] = "Gagal check-in: Parking Setting Belum Di Setup.";
                return RedirectToAction(nameof(Setting));
            }

            //Check Validation Request
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
            
            //Check If existingVehicle Is Not null
            var existingVehicle = await _vehicleRepository.GetExistingVehicleByPoliceNumber(request.PoliceNumber);
            if (existingVehicle != null)
            {
                TempData["ErrorMessage"] = "Gagal check-in: Kendaraan dengan nomor polisi yang sama sudah check-in.";
                return RedirectToAction(nameof(Index));
            }


            var getExistingVehicle = await _vehicleRepository.GetExistingVehicle();
            var availableLots = Enumerable.Range(1, Convert.ToInt16(checkMaxLot.Value)).Except(getExistingVehicle.Select(v => v.LotNumber)).ToList();

            if (availableLots.Any())
            {
                vehicle.LotNumber = availableLots.Min();
                vehicle.CheckInTime = DateTime.Now;
                var vehicle_add = await _vehicleRepository.AddVehicle(vehicle);

                var add_parking_lot = new ParkingLot
                {
                    Vehicle = vehicle_add
                };
                _ = await _parkingLotRepository.AddParkingLot(add_parking_lot);
                TempData["SuccessMessage"] = $"Check-in berhasil! Kendaraan dialokasikan ke lot {vehicle.LotNumber}.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Gagal check-in: Semua lot parkir penuh.";
            return RedirectToAction(nameof(Index));
        }

        // action used when the car exits
        [HttpPost]
        [Authorize(Policy = "CustomerServicesPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(CheckOutViewModel request)
        {
            var tempData = _tempDataFactory.GetTempData(HttpContext);

            //Check Validation Request
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

            var existingVehicle = await _vehicleRepository.GetExistingVehicleByPoliceNumber(request.PoliceNumber);
            var pricePerHour = getPricePerHour();
            if (existingVehicle != null)
            {
                existingVehicle.CheckOutTime = DateTime.Now;
                existingVehicle.pricePerHour = CalculateParkingFee(existingVehicle.CheckInTime, pricePerHour);
                _ = await _vehicleRepository.UpdateVehicle(existingVehicle);
                TempData["SuccessMessage"] = $"Check-out berhasil! Slot number {existingVehicle.LotNumber} is free";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Gagal check-out: Kendaraan dengan nomor polisi yang dimasukkan tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }
        }

        // action used when the check all reports
        [Authorize(Policy = "CustomerServicesPolicy")]
        public async Task<IActionResult> Reports()
        {
            var checkMaxLot = await _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_MAXLOT);
            var pricePerHour = getPricePerHour();
            var vehicleList = (await _vehicleRepository.GetAllVehicles())
                .Select(vehicle => new Vehicle
                {
                    PoliceNumber = vehicle.PoliceNumber,
                    Type = vehicle.Type,
                    Color = vehicle.Color,
                    CheckInTime = vehicle.CheckInTime,
                    CheckOutTime = vehicle.CheckOutTime,
                    LotNumber = vehicle.LotNumber,
                    pricePerHour = CalculateParkingFee(vehicle.CheckInTime, pricePerHour)
                })
                .ToList();

            var viewModel = new ReportsViewModel
            {
                TotalLots = checkMaxLot == null ? 0 : Convert.ToInt16(checkMaxLot.Value),
                AvailableLots = checkMaxLot == null ? 0 : Convert.ToInt16(checkMaxLot.Value) - vehicleList.Count(),
                OccupiedLots = checkMaxLot == null ? 0 : vehicleList.Count(),
                VehiclesByColors = await _reportRepository.GetDataReportVehicleGroupsByColors(),
                VehiclesByType = await _reportRepository.GetDataReportVehicleGroupsByType(),
                VehiclesByPoliceNumber = await _reportRepository.GetDataReportVehicleGroupsByTypePoliceNumber(),
                Vehicles = vehicleList
            };

            return View(viewModel);
        }

        // action used when the check all setting parking
        [Authorize(Policy = "CustomerServicesPolicy")]
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

        // action used when the update all setting parking
        [HttpPost]
        [Authorize(Policy = "CustomerServicesPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SettingUpdate(ParkingSettingViewModel request)
        {
            var tempData = _tempDataFactory.GetTempData(HttpContext);

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


        // action used when calculate the price of parking
        private decimal CalculateParkingFee(DateTime checkInTime, int pricePerHour)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan parkedDuration = currentTime - checkInTime;

            // Jika belum 1 jam, biaya tetap untuk 1 jam
            if (parkedDuration.TotalHours < 1)
            {
                return pricePerHour;
            }
            else
            {
                // Hitung jumlah jam parkir dengan pembulatan ke atas
                int totalHours = (int)Math.Ceiling(parkedDuration.TotalHours);
                return totalHours * pricePerHour;
            }
        }

        // action used when get Price From Setting Data
        private int getPricePerHour()
        {
            var checkPrice = _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_PRICE).Result;
            var checkHours = _parkingSettingRepository.GetParkingSettings(AppConstans.PARKING_SETTING_HOURS).Result;
            var checkHoursValue = checkHours != null ? Convert.ToInt16(checkHours.Value) : 1;
            var pricePerHour = 0;
            if (checkPrice != null && checkPrice.Value != "0")
            {
                pricePerHour = Convert.ToInt16(checkPrice.Value) * checkHoursValue;
            }
            return pricePerHour;
        }
    }
}
