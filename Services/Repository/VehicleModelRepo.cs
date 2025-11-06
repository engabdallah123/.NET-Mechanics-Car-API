using Data;
using Domain.DTO;
using Domain.DTO.Vehicle_Model;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class VehicleModelRepo : IVehicleModelRepo
    {
        private readonly DataContext db;

        public VehicleModelRepo(DataContext db)
        {
            this.db = db;
        }

        public async Task<List<VehicleModelDTO>> GetAllModelsAsync()
        {
            var models = await db.VehicleModels
                .Select(m => new VehicleModelDTO
                {
                    Name = m.Name,
                    Year = new YearModelDTO
                    {
                        NameYear = m.Year.YearName
                    } ,
                    Make = new MakeModelDTO
                    {
                        MakeName = m.Make.Name
                    }
                })
                .ToListAsync();

            if (!models.Any())
                throw new KeyNotFoundException("No vehicle models found.");

            return models;
        }
        public async Task<List<VehicleModelByMakeDTO>> GetModelsByMakeId(int makeId)
        {
            var models = await db.VehicleModels.Where(m => m.Make.Id == makeId)
                                  .Select(m => new VehicleModelByMakeDTO
                                  {
                                      Name = m.Name, 
                                      modelId = m.Id
                                    
                                  })
                                   .ToListAsync();
            return models;
        }
        public async Task AddModelAsync(AddVehicleModelDTO modelDTO)
        {
            if (string.IsNullOrWhiteSpace(modelDTO.Name))
                throw new ArgumentException("Model name cannot be empty.");

            var exists = await db.VehicleModels.AnyAsync(m => m.Name == modelDTO.Name);
            if (exists)
                throw new InvalidOperationException("Model already exists.");



            var model = new VehicleModel
            {
                Name = modelDTO.Name,
                YearId = modelDTO.yearId,
                MakeId = modelDTO.makeId ?? 0
            };

            await db.VehicleModels.AddAsync(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateModelAsync(int id, VehicleModelDTO modelDTO)
        {
            var model = await db.VehicleModels.FindAsync(id);
            if (model == null)
                throw new KeyNotFoundException($"Model with ID {id} not found.");

            if (!string.IsNullOrEmpty(modelDTO.Name))
                model.Name = modelDTO.Name;


            await db.SaveChangesAsync();
        }

        public async Task DeleteModelAsync(int id)
        {
            var model = await db.VehicleModels.FindAsync(id);
            if (model == null)
                throw new KeyNotFoundException($"Model with ID {id} not found.");

            db.VehicleModels.Remove(model);
            await db.SaveChangesAsync();
        }
    }
}
