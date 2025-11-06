using Domain.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DataContext()
        {
            
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<Year> Years { get; set; }
        public DbSet<Make> Make { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<RepairTask> RepairTasks { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<WorkOrderRepairTask> WorkOrderRepairTasks { get; set; }

        public DbSet<ScheduleSlot> ScheduleSlots { get; set; }
        public DbSet<ScheduleDay> ScheduleDays { get; set; }
        public DbSet<WorkStation> WorkStations { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        }
    }
}
