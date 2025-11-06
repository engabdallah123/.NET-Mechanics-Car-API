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
    public class VehicleConfiguer : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.HasOne(v => v.VehicleModel)
                   .WithMany(m => m.Vehicles)
                   .HasForeignKey(v => v.ModelId)
                   .OnDelete(DeleteBehavior.Cascade);   
        }
    }
}
