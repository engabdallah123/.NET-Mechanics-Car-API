using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int? TaskId { get; set; }

        public virtual RepairTask? RepairTask { get; set; }
    }
}
