using Domain.DTO.Work_Order;
using Domain.DTO.Work_Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Schedule_Slot
{
    public class ScheduleSlotDTO
    {
        public int slotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public bool InPast { get; set; }
        public WorkOrderDTO? WorkOrderDTO { get; set; }
    }
}
