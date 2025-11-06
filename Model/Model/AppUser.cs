using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Domain.Model
{
    public class AppUser : IdentityUser
    {
        public string? Address { get; set; }
        public virtual ICollection<WorkOrder>? WorkOrders { get; set; } = new List<WorkOrder>();
        public virtual ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new List<ScheduleSlot>();


    }
}
