using Domain.DTO.Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Repair_Task
{
    public class AddRepairTaskDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public decimal? LaborCost { get; set; }
        
    
    }
}
