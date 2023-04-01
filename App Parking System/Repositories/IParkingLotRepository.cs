using App_Parking_System.Models;
using System.Collections.Generic;

namespace App_Parking_System.Repositories
{
    public interface IParkingLotRepository
    {
        Task<IEnumerable<ParkingLot>> GetAllParkingLots();
        Task<ParkingLot> GetParkingLot(int id);
        Task<ParkingLot> AddParkingLot(ParkingLot parkingLot);
        Task<ParkingLot> UpdateParkingLot(ParkingLot parkingLot);
    }
}
