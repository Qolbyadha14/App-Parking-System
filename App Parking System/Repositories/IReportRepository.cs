using App_Parking_System.Models;

namespace App_Parking_System.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByType();
        Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByColors();
        Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByTypePoliceNumber();
    }
}
