using Domain.DTO;
using Domain.DTO.Vehicle;
using Domain.DTO.Vehicle_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IVehicleRepo
    {
        Task<List<MakeModelDTO>> GetAllVehiclesAsync();
        Task<GetVehicleDTO?> GetVehicleByIdAsync(int id);
        Task AddVehicleAsync(AddVehicleDTO vehicleDTO);
        Task UpdateVehicleAsync(int id, AddVehicleDTO vehicleDTO);
        Task DeleteVehicleAsync(int id);
    }
}
