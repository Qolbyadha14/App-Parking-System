using App_Parking_System.Models;

namespace App_Parking_System.Repositories
{
    public interface IVehicleRepository
    {
        IEnumerable<Vehicle> GetAllVehicles();
        Vehicle GetVehicle(int id);
        Vehicle AddVehicle(Vehicle vehicle);
        Vehicle UpdateVehicle(Vehicle vehicle);
    }
}
