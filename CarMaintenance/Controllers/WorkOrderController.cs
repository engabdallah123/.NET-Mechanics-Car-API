using Azure.Core;
using Domain.DTO.Work_Order;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderRepo workOrderRepo;

        public WorkOrderController(IWorkOrderRepo workOrderRepo)
        {
            this.workOrderRepo = workOrderRepo;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateWorkOrderInSlot([FromBody] AddWorkOrderDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            return Ok(new { Message = "Order Ctreated Successfuly" });
        }


        [HttpPost("/add-tasks")]
        public async Task<IActionResult> AddRepairTasksToWorkOrder([FromBody] AddRepairTasksRequest request)
        {
            await workOrderRepo.AddRepairTasksToWorkOrderAsync(request.WorkOrderId, request.RepairTaskIds);
            return Ok(new { message = "Tasks added successfully." });
        }

        [HttpPost("{workOrderId}/extend/{additionalSlots}")]
        public async Task<IActionResult> ExtendWorkOrder(int workOrderId, int additionalSlots)
        {
            if (additionalSlots <= 0)
                return BadRequest("Additional slots must be greater than zero.");

            var result = await workOrderRepo.ExtendWorkOrderAsync(workOrderId, additionalSlots);
            return Ok(result ? "Work order extended successfully." : "Failed to extend work order.");
        }

        [HttpPut("{workOrderId}/status")]
        public async Task<IActionResult> UpdateWorkOrderStatusManual(int workOrderId, [FromQuery] string technicianId, [FromQuery] WorkOrderStatus newStatus)
        {
            var result = await workOrderRepo.UpdateWorkOrderStatusManualAsync(workOrderId, technicianId, newStatus);
            return Ok(new { message = "Status updated successfully." });

        }

        [HttpPut("update-statuses/auto")]
        public async Task<IActionResult> UpdateStatusesAutomatically()
        {
            await workOrderRepo.UpdateWorkOrderStatusesAutomaticAsync();
            return Ok("Statuses updated automatically.");
        }

        [HttpGet("{workOrderId}")]
        public async Task<IActionResult> GetWorkOrderDetails(int workOrderId)
        {
            var result = await workOrderRepo.GetWorkOrderDetailsAsync(workOrderId);
            return Ok(result);
        }

        [HttpGet("{date:datetime}")]
        public async Task<IActionResult> GetWorkOrdersByDate(DateTime date)
        {
            var result = await workOrderRepo.GetWorkOrdersByDateAsync(date);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWorkOrders()
        {
            var orders = await workOrderRepo.GetAllWorkOrdersAsync();
            return Ok(orders);
        }
        
        [HttpPut("{workOrderId}/technician")]
        public async Task<IActionResult> UpdateTechnicianForWorkOrder(int workOrderId, [FromQuery] string newTechnicianId)
        {

            await workOrderRepo.UpdateTechnicianForWorkOrderAsync(workOrderId, newTechnicianId);
            return Ok( new {Message = "Technician updated successfully." });
        }
        [HttpPut("/UpdateStation")]
        public async Task<IActionResult> UpdateStationForSlots([FromBody] UpdateStationForWorkOrder request)
        {
            await workOrderRepo.UpdateStationForWorkOrderAsync(request.SlotIds,request.StationId);
            return Ok(new { Message = "Slot of station updated successfully." });

        }
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            await workOrderRepo.DeletWorkOrder(orderId);
            return Ok(new { Message = "Order Deleted Successfuly" });
        }

        // class help for adding addition tasks & Updating Station
        public class AddRepairTasksRequest
        {
            public int WorkOrderId { get; set; }
            public List<int> RepairTaskIds { get; set; } = new();
        }
        public class UpdateStationForWorkOrder
        {
            public List<int> SlotIds { get; set; } = new();
            public int StationId { get; set; }
        }

    }
}
