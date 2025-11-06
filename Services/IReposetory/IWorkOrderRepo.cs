using Domain.DTO.Work_Order;
using Domain.Enum;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IWorkOrderRepo
    {
        Task CreateWorkOrderInSlotAsync(AddWorkOrderDTO dto);
        Task<bool> ExtendWorkOrderAsync(int workOrderId, int additionalSlots);
        Task UpdateWorkOrderStatusesAutomaticAsync();
        Task<bool> UpdateWorkOrderStatusManualAsync(int workOrderId, string technicianId, WorkOrderStatus newStatus);
        Task<WorkOrderDTO> GetWorkOrderDetailsAsync(int workOrderId);
        Task<List<WorkOrderDTO>> GetWorkOrdersByDateAsync(DateTime date);
        Task<List<WorkOrderDTO>> GetAllWorkOrdersAsync();
        Task UpdateTechnicianForWorkOrderAsync(int orderId, string? newTechnicianId);
        Task UpdateStationForWorkOrderAsync(List<int> slotIds, int newStationId);
        Task AddRepairTasksToWorkOrderAsync(int workOrderId, List<int> repairTaskIds);
        Task<bool> DeletWorkOrder(int orderId);

    }
}
