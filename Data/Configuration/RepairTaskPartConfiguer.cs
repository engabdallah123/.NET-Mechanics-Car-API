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
    public class RepairTaskPartConfiguer : IEntityTypeConfiguration<RepairTask>
    {
        public void Configure(EntityTypeBuilder<RepairTask> builder)
        {
            builder.HasMany(r => r.Parts)
                .WithOne(p => p.RepairTask)
                .HasForeignKey(fk => fk.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

       
        }
    }
}
