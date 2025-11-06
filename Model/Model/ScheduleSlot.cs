using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class ScheduleSlot
    {
        public int Id { get; set; }
        public int ScheduleDayId { get; set; }
        public int WorkStationId { get; set; }
        public string? TechnicianId { get; set; }

        public TimeSpan StartTime { get; set; } 
        public TimeSpan EndTime { get; set; }
       
        public bool IsAvailable { get; set; } = true;
        public bool InPast { get; set; } = false;
        public int? WorkOrderId { get; set; }   
        public virtual WorkOrder? WorkOrder { get; set; }
        public virtual ScheduleDay ScheduleDay { get; set; }
        public virtual WorkStation WorkStation { get; set; }
        public virtual AppUser? Technician { get; set; }
    }
}
