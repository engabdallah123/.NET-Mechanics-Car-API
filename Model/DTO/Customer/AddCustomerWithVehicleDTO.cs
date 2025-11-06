using Domain.DTO.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Customer
{
    public class AddCustomerWithVehicleDTO
    {
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public List<AddVehicleDTO>? Vehiclse { get; set; }
   
    }
}
