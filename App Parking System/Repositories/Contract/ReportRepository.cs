using App_Parking_System.Data;
using App_Parking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace App_Parking_System.Repositories.Contract
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByColors()
        {
           return await _context.Vehicles
            .Where(v => v.CheckOutTime == null)
            .GroupBy(v => v.Type)
            .Select(g => new VehicleGroup
            {
                Key = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByType()
        {
            return await _context.Vehicles
                 .Where(v => v.CheckOutTime == null)
                 .GroupBy(v => v.Color)
                 .Select(g => new VehicleGroup
                 {
                     Key = g.Key.ToString(),
                     Count = g.Count()
                 })
                 .ToListAsync();
        }

        public async Task<IEnumerable<VehicleGroup>> GetDataReportVehicleGroupsByTypePoliceNumber()
        {
            var vehicles = await _context.Vehicles
                         .Where(v => v.CheckOutTime == null)
                         .ToListAsync(); // Memaksa evaluasi di sisi klien

            return vehicles
                    .GroupBy(v => ExtractNumberFromPoliceNumber(v.PoliceNumber) % 2 == 0 ? "Even" : "Odd")
                    .Select(g => new VehicleGroup
                    {
                        Key = g.Key,
                        Count = g.Count()
                    });
        }

        public int ExtractNumberFromPoliceNumber(string policeNumber)
        {
            var numberString = new string(policeNumber.Where(char.IsDigit).ToArray());
            return int.Parse(numberString);
        }
    }
}
