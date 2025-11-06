using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Data;

namespace Services
{
    public class WorkOrderStatusScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public WorkOrderStatusScheduler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                var now = DateTime.Now;

                // (1) WorkOrders pending but should have started now → InProgress
                var toStart = await db.WorkOrders
                    .Where(w => w.Status == WorkOrderStatus.Pending && w.StartTime <= now)
                    .ToListAsync();

                foreach (var order in toStart)
                {
                    order.Status = WorkOrderStatus.InProgress;
                }

                if (toStart.Any())
                    await db.SaveChangesAsync();

                // (2) Repeat every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

    }
}
