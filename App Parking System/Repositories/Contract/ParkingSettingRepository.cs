using App_Parking_System.Data;
using App_Parking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace App_Parking_System.Repositories.Contract
{
    public class ParkingSettingRepository : IParkingSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public ParkingSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingSettings> AddParkingSettings(ParkingSettings parkingSettings)
        {
            _context.AddAsync(parkingSettings);
            await _context.SaveChangesAsync();
            return parkingSettings;
        }

        public async Task<ParkingSettings> GetParkingSettings(string Code)
        {
            try
            {
                return await _context.ParkingSettings.Where(x => x.Code == Code).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ParkingSettings> UpdateParkingSettings(ParkingSettings parkingSettings)
        {
            var result = await _context.ParkingSettings.Where(x => x.Code == parkingSettings.Code).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Value = parkingSettings.Value;
                result.Name = parkingSettings.Name;
                await _context.SaveChangesAsync();
            }

            return parkingSettings;
        }
    }
}
