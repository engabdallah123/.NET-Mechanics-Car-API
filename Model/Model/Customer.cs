using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Customer
    {
  
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public virtual List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual List<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

    }
}
