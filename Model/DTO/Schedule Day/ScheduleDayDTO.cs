using Domain.DTO.Work_Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Schedule_Day
{
    public class ScheduleDayDTO
    {
        public DateTime Date { get; set; }
        public List<WorkStationDTO> Stations { get; set; } = new();
    }
}
