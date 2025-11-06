using Data;
using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class RepairTaskRepo : IRepairTaskRepo
    {
        private readonly DataContext db;

        public RepairTaskRepo(DataContext db)
        {
            this.db = db;
        }
        public async Task<List<RepairTaskDTO>> GetAllRepairTask()
        {
            return await db.RepairTasks.Include(r => r.Parts)
                .Select(r => new RepairTaskDTO
                {
                    Name = r.Name,
                    TaskId = r.Id,
                    Description = r.Description,
                    Duration = r.EstimatedDuration,
                    LaborCost = r.LaborCost,                  
                    Parts = r.Parts.Select(p => new PartDTO
                    {
                        Name = p.Name,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    }).ToList()
                }).ToListAsync();
        }
        public async Task<RepairTaskDTO> GetRepairTaskById(int id)
        {
            var task = await db.RepairTasks.Include(r => r.Parts).FirstOrDefaultAsync(r => r.Id == id);
            if (task == null)
                throw new KeyNotFoundException($"Task With id '{id}' not found");

            return new RepairTaskDTO
            {
                Name = task.Name,
                TaskId = task.Id,
                Description = task.Description,
                Duration = task.EstimatedDuration,
                LaborCost = task.LaborCost,
                Parts = task.Parts.Select(p => new PartDTO
                {
                    Name = p.Name,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                }).ToList()
            };
        }
        public async Task AddTask(AddRepairTaskDTO taskDTO)
        {
            var task = new RepairTask
            {
                Name = taskDTO.Name ?? string.Empty,
                Description = taskDTO.Description ?? string.Empty,
                EstimatedDuration = taskDTO.Duration ?? 0,
                LaborCost = taskDTO.LaborCost ?? 0.00m,

            };
            await db.RepairTasks.AddAsync(task);
            await db.SaveChangesAsync();
        }

        public async Task CreateRepairTask(AddRepairTaskWithPartDTO taskWithPart)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var task = new RepairTask
                {
                    Name = taskWithPart.taskDTO.Name ?? string.Empty,
                    Description = taskWithPart.taskDTO.Description ?? string.Empty,
                    EstimatedDuration = taskWithPart.taskDTO.Duration ?? 0,
                    LaborCost = taskWithPart.taskDTO.LaborCost ?? 0.00m,
                };

                await db.RepairTasks.AddAsync(task);
                await db.SaveChangesAsync();

                var parts = taskWithPart.Parts.Select(p => new Part
                {
                    TaskId = task.Id,
                    Name = p.Name ?? string.Empty,
                    UnitPrice = p.UnitPrice ?? 0.00m,
                    Quantity = p.Quantity ?? 1,

                }).ToList();

                await db.Parts.AddRangeAsync(parts);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateRepairTask(int id, AddRepairTaskWithPartDTO taskWithPart)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
               
                var task = await db.RepairTasks
                    .Include(r => r.Parts)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (task == null)
                    throw new KeyNotFoundException($"Task with ID {id} not found.");


                task.Parts ??= new List<Part>();

                if (taskWithPart.taskDTO != null)
                {
                    task.Name = taskWithPart.taskDTO.Name ?? task.Name;
                    task.EstimatedDuration = taskWithPart.taskDTO.Duration ?? task.EstimatedDuration;
                    task.Description = taskWithPart.taskDTO.Description ?? task.Description;
                    task.LaborCost = taskWithPart.taskDTO.LaborCost ?? task.LaborCost;
                }


                if (taskWithPart.Parts != null && taskWithPart.Parts.Any())
                {               
                    foreach (var pDto in taskWithPart.Parts)
                    {
                        if (pDto.Id.HasValue)
                        {
                           
                            var existingPart = task.Parts.FirstOrDefault(p => p.Id == pDto.Id.Value);
                            if (existingPart != null)
                            {
                                existingPart.Name = pDto.Name ?? existingPart.Name;
                                existingPart.Quantity = pDto.Quantity ?? existingPart.Quantity;
                                existingPart.UnitPrice = pDto.UnitPrice ?? existingPart.UnitPrice;
                            }
                        }
                        else
                        {

                            var parts = taskWithPart.Parts.Select(p => new Part
                            {
                                TaskId = task.Id,
                                Name = p.Name ?? string.Empty,
                                UnitPrice = p.UnitPrice ?? 0.00m,
                                Quantity = p.Quantity ?? 1,

                            }).ToList();

                            await db.Parts.AddRangeAsync(parts);
                        }
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteRepairTask(int id)
        {
            var task = await db.RepairTasks.Include(r => r.Parts)
                .Where(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException($"Task With id '{id}' not found");
            db.RepairTasks.Remove(task);
            await db.SaveChangesAsync();
        }


    }
}
