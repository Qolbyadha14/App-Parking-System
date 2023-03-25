using App_Parking_System.Data;
using App_Parking_System.Models;

namespace App_Parking_System.Repositories.Contract
{
    public class ParkingLotRepository : IParkingLotRepository
    {
        private readonly ApplicationDbContext _context;

        public ParkingLotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ParkingLot> GetAllParkingLots()
        {
            return _context.ParkingLots.ToList();
        }

        public ParkingLot GetParkingLot(int id)
        {
            return _context.ParkingLots.Find(id);
        }

        public ParkingLot AddParkingLot(ParkingLot parkingLot)
        {
            _context.ParkingLots.Add(parkingLot);
            _context.SaveChanges();
            return parkingLot;
        }

        public ParkingLot UpdateParkingLot(ParkingLot parkingLot)
        {
            _context.ParkingLots.Update(parkingLot);
            _context.SaveChanges();
            return parkingLot;
        }
    }
}
