using App_Parking_System.Models;

namespace App_Parking_System.ViewModels
{
    public class ReportsViewModel
    {
        public int TotalLots { get; set; }
        public int AvailableLots { get; set; }
        public int OccupiedLots { get; set; }
        public IEnumerable<VehicleGroup> VehiclesByPoliceNumber { get; set; }
        public IEnumerable<VehicleGroup> VehiclesByColors { get; set; }
        public IEnumerable<VehicleGroup> VehiclesByType { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}
