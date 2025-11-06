using Data;
using Domain.DTO;
using Domain.DTO.Vehicle;
using Domain.DTO.Vehicle_Model;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class VehicleRepo : IVehicleRepo
    {
        private readonly DataContext db;

        public VehicleRepo(DataContext db)
        {
            this.db = db;
        }

        public async Task<List<MakeModelDTO>> GetAllVehiclesAsync()
        {
            return await db.Make
                .Select(v => new MakeModelDTO
                {
                    MakeName = v.Name,
                    MakeId = v.Id

                })
                .ToListAsync();
        }

        public async Task<GetVehicleDTO?> GetVehicleByIdAsync(int id)
        {
            var vehicle = await db.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return null;

            return new GetVehicleDTO
            {
                
                LicensePlate = vehicle.LicensePlate,
                CustomerName = vehicle.Customer?.Name,
                VehicleModel = new VehicleModelDTO
                {
                    Name = vehicle.VehicleModel.Name,
                    VehicleId = vehicle.Id,
                    Year = new YearModelDTO
                    {
                        NameYear = vehicle.VehicleModel.Year.YearName
                    },
                    Make = new MakeModelDTO
                    {
                        MakeName = vehicle.VehicleModel.Make.Name
                    }
                }
            };
        }

        public async Task AddVehicleAsync(AddVehicleDTO vehicleDTO)
        {
            var model = await db.VehicleModels
                .FirstOrDefaultAsync(m => m.Id == vehicleDTO.ModelId);

            if (model == null)
                throw new KeyNotFoundException($"Model with ID '{vehicleDTO.ModelId}' not found.");

            var vehicle = new Vehicle
            {
                
                LicensePlate = vehicleDTO.LicensePlate ?? string.Empty,
                ModelId = vehicleDTO.ModelId ,
                
            };

            await db.Vehicles.AddAsync(vehicle);
            await db.SaveChangesAsync();
        }



        public async Task UpdateVehicleAsync(int id, AddVehicleDTO vehicleDTO)
        {
            var vehicle = await db.Vehicles.FindAsync(id);
            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with id {id} not found");

            
            vehicle.LicensePlate = vehicleDTO.LicensePlate ?? vehicle.LicensePlate;

            await db.SaveChangesAsync();
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await db.Vehicles.FindAsync(id);
            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with id {id} not found");

            db.Vehicles.Remove(vehicle);
            await db.SaveChangesAsync();
        }
    }
}
