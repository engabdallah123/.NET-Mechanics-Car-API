using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string? LicensePlate { get; set; }
        public int? CustomerId { get; set; }
        public int? ModelId { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<WorkOrder>? WorkOrders { get; set; } = new List<WorkOrder>();
        public virtual VehicleModel VehicleModel { get; set; } 

    }
}
