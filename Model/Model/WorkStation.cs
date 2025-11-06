using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class WorkStation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; } // ex: WS-1, WS-2 ...



        public virtual ICollection<ScheduleSlot> Slots { get; set; } = new List<ScheduleSlot>();
    }
}
