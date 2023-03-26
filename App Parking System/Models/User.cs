using Microsoft.AspNetCore.Identity;

namespace App_Parking_System.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
