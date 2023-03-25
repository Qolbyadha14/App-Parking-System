using App_Parking_System.Data;
using App_Parking_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace App_Parking_System.Repositories.Contract
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>>  GetAllVehicles()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle> GetVehicle(int id)
        {
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task<Vehicle> AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> GetExistingVehicleByPoliceNumber(string PoliceNumber)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(v => v.PoliceNumber == PoliceNumber && v.CheckOutTime == null);
        }

        public async Task<List<Vehicle>> GetExistingVehicle()
        {
            return await _context.Vehicles.Where(v => v.CheckOutTime == null).ToListAsync();
        }
    }
}
