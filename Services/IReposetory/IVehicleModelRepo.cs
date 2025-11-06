using Domain.DTO;
using Domain.DTO.Vehicle_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IVehicleModelRepo
    {
        Task<List<VehicleModelDTO>> GetAllModelsAsync();
        Task<List<VehicleModelByMakeDTO>> GetModelsByMakeId(int makeId);

        Task AddModelAsync(AddVehicleModelDTO modelDTO);
        Task UpdateModelAsync(int id, VehicleModelDTO modelDTO);
        Task DeleteModelAsync(int id);
    }
}
