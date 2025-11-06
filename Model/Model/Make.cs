using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Make
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<VehicleModel>? VehicleModels { get; set; } = new List<VehicleModel>();
    }
}
