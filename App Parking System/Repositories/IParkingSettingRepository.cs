using App_Parking_System.Models;

namespace App_Parking_System.Repositories
{
    public interface IParkingSettingRepository
    {
        Task<ParkingSettings> GetParkingSettings(string Code);
        Task<ParkingSettings> AddParkingSettings(ParkingSettings parkingSettings);
        Task<ParkingSettings> UpdateParkingSettings(ParkingSettings parkingSettings);
    }
}
