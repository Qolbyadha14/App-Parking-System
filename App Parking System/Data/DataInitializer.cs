using App_Parking_System.Models;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App_Parking_System.Data
{
    public class DataInitializer
    {
        public static async Task InitializeAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Cek apakah role "Admin" sudah ada
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                // Buat role "Admin" jika belum ada
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Cek apakah role "Admin" sudah ada
            if (!await roleManager.RoleExistsAsync("CS"))
            {
                // Buat role "Admin" jika belum ada
                await roleManager.CreateAsync(new IdentityRole("CS"));
            }

            // Cek apakah user admin sudah ada
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Buat user admin jika belum ada
                adminUser = new User { 
                    UserName = adminEmail, 
                    Email = adminEmail,
                    Address = "No Address",
                    FirstName = "admin",
                    ProfilePictureUrl = "No Image",
                    LastName = "Super"
                };
                var adminPassword = "Admin123!";
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    // Menambahkan role "Admin" ke user yang baru dibuat
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Cek apakah user cs sudah ada
            var csEmail = "cs@example.com";

            var csUser = await userManager.FindByEmailAsync(csEmail);
            if (csUser == null)
            {
                // Buat user admin jika belum ada
                csUser = new User
                {
                    UserName = csEmail,
                    Email = csEmail,
                    Address = "No Address",
                    FirstName = "Customer",
                    ProfilePictureUrl = "No Image",
                    LastName = "Services"
                };
                var csPassword = "CustServ123!";
                var createResult = await userManager.CreateAsync(csUser, csPassword);
                if (createResult.Succeeded)
                {
                    // Menambahkan role "Admin" ke user yang baru dibuat
                    await userManager.AddToRoleAsync(csUser, "Admin");
                }
            }
        }
    }
}
