using Domain.DTO.Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Repair_Task
{
    public class RepairTaskDTO
    {
        public string Name { get; set; }
        public int TaskId { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public decimal LaborCost { get; set; }
        public decimal TotalCost => LaborCost + Parts.Sum(x => x.Quantity * x.UnitPrice);
        public List<PartDTO> Parts { get; set; }

    }
}
