using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Year
    {
        public int Id { get; set; }
        public string YearName { get; set; }

        public virtual ICollection<VehicleModel> Models { get; set; }
    }
}
