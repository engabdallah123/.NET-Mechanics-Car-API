using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Part
{
    public class AddPartDTO
    {
        public string? Name { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public int? TaskId { get; set; }
        public int? Id { get; set; }

    }
}
