using Domain.DTO.Repair_Task;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Work_Order
{
    public class AddWorkOrderDTO
    {
        public int? VehicleId { get; set; }
        public string? TechnicianId { get; set; }
        public int CustomerId { get; set; }
        public int SlotId { get; set; }
        public List<AddRepairTaskToOrderDTO> RepairTasks { get; set; } = new();

    }
}
