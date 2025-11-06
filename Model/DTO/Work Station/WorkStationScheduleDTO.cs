using Domain.DTO.Schedule_Slot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Work_Station
{
    public class WorkStationScheduleDTO
    {
        public int WorkStationId { get; set; }
        public string WorkStationName { get; set; }
        public string Code { get; set; }
        public List<SlotStatusDTO> Slots { get; set; } = new();
    }
}
