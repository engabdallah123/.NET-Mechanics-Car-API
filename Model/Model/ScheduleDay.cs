using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class ScheduleDay
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
       
        public virtual ICollection<ScheduleSlot> Slots { get; set; } = new List<ScheduleSlot>();
    }
}
