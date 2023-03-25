using App_Parking_System.Models;
using System.Collections.Generic;

namespace App_Parking_System.Repositories
{
    public interface IParkingLotRepository
    {
        IEnumerable<ParkingLot> GetAllParkingLots();
        ParkingLot GetParkingLot(int id);
        ParkingLot AddParkingLot(ParkingLot parkingLot);
        ParkingLot UpdateParkingLot(ParkingLot parkingLot);
    }
}
