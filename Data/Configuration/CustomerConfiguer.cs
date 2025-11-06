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
    public class CustomerConfiguer : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Vehicles)
                   .WithOne(v => v.Customer)
                   .HasForeignKey(v => v.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.WorkOrders)
                     .WithOne(wo => wo.Customer)
                     .HasForeignKey(wo => wo.CustomerId)
                     .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
