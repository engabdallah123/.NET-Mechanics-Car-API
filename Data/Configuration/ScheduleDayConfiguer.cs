using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Data.Configuration
{
    public class ScheduleDayConfiguer : IEntityTypeConfiguration<ScheduleDay>
    {
        public void Configure(EntityTypeBuilder<ScheduleDay> builder)
        {
 

            builder.HasMany(d => d.Slots)
                .WithOne(s => s.ScheduleDay)
                .HasForeignKey(fk => fk.ScheduleDayId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
