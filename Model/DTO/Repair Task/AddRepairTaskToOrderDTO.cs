using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Repair_Task
{
    public class AddRepairTaskToOrderDTO
    {
        public int RepairTaskId { get; set; }
        public int Duration { get; set; }
    }
}
