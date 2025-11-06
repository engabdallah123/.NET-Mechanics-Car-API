using Domain.DTO.Schedule_Slot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Work_Station
{
    public class WorkStationDTO
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public int stactionId { get; set; }
        public List<ScheduleSlotDTO> Slots { get; set; } = new();
    }
}
