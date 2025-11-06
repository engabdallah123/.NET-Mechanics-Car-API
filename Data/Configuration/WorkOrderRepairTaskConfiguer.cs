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
    public class WorkOrderRepairTaskConfiguer : IEntityTypeConfiguration<WorkOrderRepairTask>
    {
        public void Configure(EntityTypeBuilder<WorkOrderRepairTask> builder)
        {
            builder.HasKey(wrt => new { wrt.WorkOrderId, wrt.RepairTaskId });

            builder.HasOne(wrt => wrt.WorkOrder)
                   .WithMany(wo => wo.WorkOrderRepairTasks)
                   .HasForeignKey(wrt => wrt.WorkOrderId);

            builder.HasOne(wrt => wrt.RepairTask)
                   .WithMany(rt => rt.WorkOrderRepairTasks)
                   .HasForeignKey(wrt => wrt.RepairTaskId);
        }
    }

}
