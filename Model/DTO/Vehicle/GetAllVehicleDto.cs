using Domain.DTO.Vehicle_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Vehicle
{
    public class GetAllVehicleDto
    {
        public int Id { get; set; }
        public MakeModelDTO? Make { get; set; }
    }
}
