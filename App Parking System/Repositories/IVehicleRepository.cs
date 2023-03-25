using App_Parking_System.Models;

namespace App_Parking_System.Repositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllVehicles();
        Task<Vehicle> GetVehicle(int id);
        Task<Vehicle> GetExistingVehicleByPoliceNumber(string PoliceNumber);
        Task<List<Vehicle>> GetExistingVehicle();
        Task<Vehicle> AddVehicle(Vehicle vehicle);
        Task<Vehicle> UpdateVehicle(Vehicle vehicle);
    }
}
