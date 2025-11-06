using Domain.DTO.Invoice;
using Domain.DTO.Repair_Task;
using Domain.DTO.Schedule_Slot;
using Domain.DTO.User;
using Domain.DTO.Vehicle;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Work_Order
{
    public class WorkOrderDTO
    {
        public string Code { get; set; }
        public int orderId { get; set; }
        public int Duration { get; set; }
        public DateTime Scheduled {  get; set; }
        public List<int>? slotIds { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; }
        public string TechnicianName { get; set; }
        public string TecnicianId { get; set; }
        public string WorkStationName { get; set; }
        public int WorkStationId { get; set; }
        public getInvoiceOrder Invoice { get; set; }
        
        
        public decimal GrandTotal => (RepairTasks?.Sum(rt =>(rt?.LaborCost ?? 0) + (rt?.Parts?.Sum(p => (p?.Quantity ?? 0) * (p?.UnitPrice ?? 0)) ?? 0)) ?? 0);
        public List<RepairTaskDTO> RepairTasks { get; set; } = new();
        public GetVehicleDTO Vehicle { get; set; }
    }
}
