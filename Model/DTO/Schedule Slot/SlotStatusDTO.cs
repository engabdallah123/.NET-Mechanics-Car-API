using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Schedule_Slot
{
    public class SlotStatusDTO
    {
        public int SlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? TechnicianName { get; set; }
        public bool IsAvailable { get; set; }
        public int? WorkOrderId { get; set; }
        public string Status { get; set; } = "Empty";
    }
}
