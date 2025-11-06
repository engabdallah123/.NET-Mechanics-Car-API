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
    public class ModelConfiguer
    {
        public class CustomerConfiguer : IEntityTypeConfiguration<VehicleModel>
        {
            public void Configure(EntityTypeBuilder<VehicleModel> builder)
            {
                builder.HasOne(m => m.Year)
                    .WithMany(Year => Year.Models)
                    .HasForeignKey(m => m.YearId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(m => m.Make)
                    .WithMany(Make => Make.VehicleModels)
                    .HasForeignKey(m => m.MakeId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
}
