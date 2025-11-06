using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public string? TechnicianId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public WorkOrderStatus Status { get; set; }

        public virtual ICollection<ScheduleSlot>? Slots { get; set; } = new List<ScheduleSlot>();
        public virtual Customer Customer { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual AppUser? Technician { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual ICollection<WorkOrderRepairTask>? WorkOrderRepairTasks { get; set; } = new List<WorkOrderRepairTask>();
    }
}
