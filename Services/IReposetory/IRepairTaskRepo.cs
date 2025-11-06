using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IRepairTaskRepo
    {
        Task<List<RepairTaskDTO>> GetAllRepairTask();
        Task<RepairTaskDTO> GetRepairTaskById(int id);
        Task CreateRepairTask(AddRepairTaskWithPartDTO taskWithPart);
        Task UpdateRepairTask(int id, AddRepairTaskWithPartDTO taskWithPart);
        Task DeleteRepairTask(int id);
    }
}
