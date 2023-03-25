namespace App_Parking_System.Models
{
    public class Vehicle
    {
        public int? Id { get; set; }
        public string PoliceNumber { get; set; }
        public VehicleType Type { get; set; }
        public string Color { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int LotNumber { get; set; }

    }

    public enum VehicleType
    {
        Car,
        Motorbike
    }

    public class VehicleGroup
    {
        public string Key { get; set; }
        public int Count { get; set; }
    }
}
