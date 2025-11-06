using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Vehicle
{
    public class GetVehicleDTO
    {
        public int Id { get; set; }
        public string? LicensePlate { get; set; }
        public string? CustomerName { get; set; }
        public VehicleModelDTO VehicleModel { get; set; }
    }
}
