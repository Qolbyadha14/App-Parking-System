using App_Parking_System.Data;
using App_Parking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace App_Parking_System.Repositories.Contract
{
    public class ParkingLotRepository : IParkingLotRepository
    {
        private readonly ApplicationDbContext _context;

        public ParkingLotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public  async Task<IEnumerable<ParkingLot>> GetAllParkingLots()
        {
            return await _context.ParkingLots.ToListAsync();
        }

        public async Task<ParkingLot> GetParkingLot(int id)
        {
            return await _context.ParkingLots.FindAsync(id);
        }

        public async Task<ParkingLot> AddParkingLot(ParkingLot parkingLot)
        {
            _context.ParkingLots.Add(parkingLot);
            await _context.SaveChangesAsync();
            return parkingLot;
        }

        public async Task<ParkingLot> UpdateParkingLot(ParkingLot parkingLot)
        {
            _context.ParkingLots.Update(parkingLot);
            await _context.SaveChangesAsync();
            return parkingLot;
        }
    }
}
