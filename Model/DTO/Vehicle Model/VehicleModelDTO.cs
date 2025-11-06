using Domain.DTO.Vehicle_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class VehicleModelDTO
    {

        public string? Name { get; set; }
        public int? VehicleId { get;  set; }
        public YearModelDTO? Year { get; set; }
        public MakeModelDTO? Make { get; set; }


    }
}
