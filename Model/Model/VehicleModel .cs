using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public int? YearId { get; set; }
        public virtual List<Vehicle> Vehicles { get; set; } = new();
        public virtual Year Year { get; set; }
        public int? MakeId { get; set; }
        public virtual Make Make { get; set; }
    }
}
