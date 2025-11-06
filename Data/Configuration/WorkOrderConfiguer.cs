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
    public class WorkOrderConfiguer : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.HasKey(wo => wo.Id);

            builder.HasOne(wo => wo.Vehicle)
                   .WithMany(v => v.WorkOrders)
                   .HasForeignKey(wo => wo.VehicleId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(w => w.TechnicianId).IsRequired(false);

            builder.HasOne(wo => wo.Technician)
                   .WithMany(t => t.WorkOrders)
                   .HasForeignKey(wo => wo.TechnicianId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(wo => wo.Invoice)
                   .WithOne(i => i.WorkOrder)
                   .HasForeignKey<Invoice>(i => i.WorkOrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(w => w.Slots)
                 .WithOne(s => s.WorkOrder)
                 .HasForeignKey(fk => fk.WorkOrderId)
                 .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
