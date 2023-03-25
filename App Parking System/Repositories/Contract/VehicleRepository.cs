using App_Parking_System.Data;
using App_Parking_System.Models;

namespace App_Parking_System.Repositories.Contract
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Vehicle> GetAllVehicles()
        {
            return _context.Vehicles.ToList();
        }

        public Vehicle GetVehicle(int id)
        {
            return _context.Vehicles.Find(id);
        }

        public Vehicle AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
            return vehicle;
        }

        public Vehicle UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
            return vehicle;
        }
    }
}
