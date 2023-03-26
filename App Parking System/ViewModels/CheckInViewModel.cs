using App_Parking_System.Models;
using System.ComponentModel.DataAnnotations;

namespace App_Parking_System.ViewModels
{
    public class CheckInViewModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z]{1}-\d{4}-[A-Za-z]{3}$", ErrorMessage = "Format nomor polisi tidak valid. Contoh: B-1234-XYZ")]
        public string PoliceNumber { get; set; }
        public VehicleType Type { get; set; }
        public string Color { get; set; }
    }
}
