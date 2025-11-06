using Domain.DTO.Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Repair_Task
{
    public class AddRepairTaskWithPartDTO
    {
        public AddRepairTaskDTO? taskDTO {  get; set; }
        public List<AddPartDTO>? Parts { get; set; }
    }
}
