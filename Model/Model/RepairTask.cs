using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class RepairTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int EstimatedDuration { get; set; }
        public decimal LaborCost { get; set; }
        public virtual ICollection<WorkOrderRepairTask>? WorkOrderRepairTasks { get; set; } = new List<WorkOrderRepairTask>();
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
