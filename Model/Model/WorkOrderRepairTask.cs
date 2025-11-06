using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class WorkOrderRepairTask
    {
        public int WorkOrderId { get; set; }
        public int RepairTaskId { get; set; }
        public int? Duration { get; set; }

        public virtual WorkOrder WorkOrder { get; set; } = null!;
        public virtual RepairTask RepairTask { get; set; } = null!;

        
    }
}
