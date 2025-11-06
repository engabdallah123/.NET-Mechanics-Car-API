using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Configuration
{
    public class ScheduleSlotConfiguer : IEntityTypeConfiguration<ScheduleSlot>
    {
        public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
        {
            
            builder.HasKey(ss => ss.Id);

            builder.HasOne(s => s.Technician)
                .WithMany(t => t.ScheduleSlots)
                .HasForeignKey(f => f.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.WorkStation)
                .WithMany(w => w.Slots)
                .HasForeignKey(f => f.WorkStationId)
                .OnDelete(DeleteBehavior.Restrict);
           
        }
    }
}
