using Domain.DTO.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Customer
{
    public class GetCustomerWithVehicleDTO
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int customerId { get; set; }
        public List<GetVehicleDTO> Vehicles { get; set; }
    }
}
