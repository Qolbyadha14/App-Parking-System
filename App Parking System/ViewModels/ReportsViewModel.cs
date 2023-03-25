using App_Parking_System.Models;

namespace App_Parking_System.ViewModels
{
    public class ReportsViewModel
    {
        public int TotalLots { get; set; }
        public int AvailableLots { get; set; }
        public List<VehicleGroup> VehiclesByPoliceNumber { get; set; }
        public List<VehicleGroup> VehiclesByColors { get; set; }
        public List<VehicleGroup> VehiclesByType { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}
