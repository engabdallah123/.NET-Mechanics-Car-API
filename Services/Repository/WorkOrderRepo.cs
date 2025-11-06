using Data;
using Domain.DTO;
using Domain.DTO.Invoice;
using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.DTO.Vehicle;
using Domain.DTO.Vehicle_Model;
using Domain.DTO.Work_Order;
using Domain.Enum;
using Domain.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class WorkOrderRepo : IWorkOrderRepo
    {
        private readonly DataContext db;

        public WorkOrderRepo(DataContext db)
        {
            this.db = db;
        }
        public async Task CreateWorkOrderInSlotAsync(AddWorkOrderDTO dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var slot = await db.ScheduleSlots
                    .FirstOrDefaultAsync(s => s.Id == dto.SlotId);

                if (slot == null)
                    throw new KeyNotFoundException("Slot not found.");

                if (!slot.IsAvailable)
                    throw new InvalidOperationException("This slot is already reserved.");

                // (1) Create WorkOrder (initially with slot times)
                var workOrder = new WorkOrder
                {
                    CustomerId = dto.CustomerId,
                    VehicleId = dto.VehicleId,
                    TechnicianId = dto.TechnicianId,
                    Status = WorkOrderStatus.Pending,
                    StartTime = slot.ScheduleDay.Date.Add(slot.StartTime),
                    EndTime = slot.ScheduleDay.Date.Add(slot.StartTime) // we'll adjust this later
                };

                await db.WorkOrders.AddAsync(workOrder);
                await db.SaveChangesAsync();

                // (2) Link WorkOrder with Slot
                slot.WorkOrderId = workOrder.Id;
                slot.TechnicianId = dto.TechnicianId;
                slot.IsAvailable = false;
                await db.SaveChangesAsync();

                // (3) Add Repair Tasks
                int totalDuration = 0;

                if (dto.RepairTasks != null && dto.RepairTasks.Any())
                {
                    foreach (var repairTaskDto in dto.RepairTasks)
                    {
                        var repairTask = await db.RepairTasks.FindAsync(repairTaskDto.RepairTaskId);
                        if (repairTask == null)
                            throw new KeyNotFoundException($"RepairTask {repairTaskDto.RepairTaskId} not found.");

                        int duration = repairTaskDto.Duration != 0 ? repairTaskDto.Duration : repairTask.EstimatedDuration;
                        totalDuration += duration;

                        var workOrderRepairTask = new WorkOrderRepairTask
                        {
                            WorkOrderId = workOrder.Id,
                            RepairTaskId = repairTask.Id,
                            Duration = duration
                        };

                        await db.WorkOrderRepairTasks.AddAsync(workOrderRepairTask);
                    }

                    await db.SaveChangesAsync();
                }

                // (4) Adjust EndTime based on total duration of tasks
                if (totalDuration > 0)
                {
                    workOrder.EndTime = workOrder.StartTime.AddMinutes(totalDuration);
                    await db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> ExtendWorkOrderAsync(int workOrderId, int additionalSlots)
        {
            var workOrder = await db.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException("WorkOrder not found.");

            var lastSlot =  workOrder.Slots?.OrderByDescending(s => s.EndTime).First();

            // get the slots that in the same station and same day  
            var nextSlots = await db.ScheduleSlots
                .Where(s =>
                    s.ScheduleDayId == lastSlot.ScheduleDayId &&
                    s.WorkStationId == lastSlot.WorkStationId &&
                    s.StartTime >= lastSlot.EndTime &&
                    s.IsAvailable)
                .OrderBy(s => s.StartTime)
                .Take(additionalSlots)
                .ToListAsync();

            if (nextSlots.Count < additionalSlots)
                throw new InvalidOperationException("Not enough available slots to extend order.");

            // connect
            foreach (var slot in nextSlots)
            {
                slot.WorkOrderId = workOrder.Id;
                slot.TechnicianId = workOrder.TechnicianId;
                slot.IsAvailable = false;
            }

            // update end time
            workOrder.EndTime = workOrder.EndTime.AddMinutes(15 * additionalSlots);

            await db.SaveChangesAsync();
            return true;
        }
        public async Task UpdateWorkOrderStatusesAutomaticAsync()
        {
            var now = DateTime.Now;

            var orders = await db.WorkOrders.ToListAsync();

            foreach (var order in orders)
            {
                if (order.Status == WorkOrderStatus.Completed) continue;

                if (now >= order.EndTime)
                    order.Status = WorkOrderStatus.Completed;
                else if (now >= order.StartTime)
                    order.Status = WorkOrderStatus.InProgress;
                else
                    order.Status = WorkOrderStatus.Pending;
            }

            await db.SaveChangesAsync();
        }
        public async Task<bool> UpdateWorkOrderStatusManualAsync(int workOrderId, string technicianId, WorkOrderStatus newStatus)
        {
            var workOrder = await db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException("WorkOrder not found.");

            if (workOrder.TechnicianId != technicianId)
                throw new UnauthorizedAccessException("You are not authorized to update this work order.");

           
            var allowedStatuses = new[]
            {
                WorkOrderStatus.Pending,
                WorkOrderStatus.InProgress,
               WorkOrderStatus.Completed
            };

            if (!allowedStatuses.Contains(newStatus))
                throw new InvalidOperationException("Invalid status transition.");

            
            workOrder.Status = newStatus;

            await db.SaveChangesAsync();
            return true;
        }


        public async Task<WorkOrderDTO> GetWorkOrderDetailsAsync(int workOrderId)
        {
            var workOrder = await db.WorkOrders   
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException("WorkOrder not found.");

            var slotIds = workOrder.Slots?.Select(s => s.Id).ToList() ?? new List<int>();

            var firstSlot = workOrder.Slots?.OrderBy(s => s.StartTime).FirstOrDefault();
            var lastSlot = workOrder.Slots?.OrderByDescending(s => s.EndTime).FirstOrDefault();

            var startTime = firstSlot?.StartTime;
           
            double totalTasksDuration = workOrder.WorkOrderRepairTasks?.Sum(rt => rt.Duration ?? 0) ?? 0;

            TimeSpan endTime;
            if (startTime.HasValue)
                endTime = startTime.Value.Add(TimeSpan.FromMinutes(totalTasksDuration));
            else
                endTime = lastSlot?.EndTime ?? TimeSpan.Zero;


            var totalDuration = (int)(endTime - (startTime ?? TimeSpan.Zero)).TotalMinutes;

            var dto = new WorkOrderDTO
            {
                Code = $"WO-{workOrder.Id:D5}",
                orderId = workOrder.Id,
                Duration = totalDuration,
                Scheduled = firstSlot != null ? firstSlot.ScheduleDay.Date : workOrder.StartTime.Date,
                slotIds = slotIds, 
                Status = workOrder.Status.ToString(),
                TechnicianName = workOrder.Technician?.UserName ?? "Unassigned",
                TecnicianId = workOrder.Technician?.Id ?? "0",
                WorkStationName = firstSlot?.WorkStation?.Name ?? "Not assigned",
                WorkStationId = firstSlot?.WorkStation?.Id ?? 0,
                StartTime = (TimeSpan)startTime,
                EndTime = endTime,
                Invoice = workOrder.Invoice != null
                                 ? new getInvoiceOrder
                                 {
                                     InvoiceId = workOrder.Invoice.Id,
                                     IsPaid = workOrder.Invoice.IsPaid
                                 }
                                    : null,

                Vehicle = new GetVehicleDTO
                {
                    Id = workOrder.Vehicle.Id,
                    LicensePlate = workOrder.Vehicle.LicensePlate,
                    CustomerName = workOrder.Customer.Name,
                    VehicleModel = new VehicleModelDTO
                    {
                        Name = workOrder.Vehicle.VehicleModel?.Name ?? "N/A",
                        VehicleId = workOrder.Vehicle.Id,
                        Year = new YearModelDTO
                        {
                            NameYear = workOrder.Vehicle.VehicleModel.Year.YearName
                        },
                        Make = new MakeModelDTO
                        {
                            MakeName = workOrder.Vehicle.VehicleModel.Make.Name
                        }

                    },
                },

                RepairTasks = workOrder.WorkOrderRepairTasks.Select(rt => new RepairTaskDTO
                {
                    Name = rt.RepairTask.Name,
                    Description = rt.RepairTask.Description,
                    LaborCost = rt.RepairTask.LaborCost,
                    Duration = rt.Duration ?? 0,
                    Parts = rt.RepairTask.Parts.Select(p => new PartDTO
                    {
                        Name = p.Name,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    }).ToList()
                }).ToList()
            };

            return dto;
        }
        public async Task<List<WorkOrderDTO>> GetWorkOrdersByDateAsync( DateTime date )
        {
            var workOrders = await db.WorkOrders.Where(w =>
                       w.Slots.Any(s => s.ScheduleDay.Date.Date == date.Date)
                     || w.StartTime.Date == date.Date).ToListAsync();

            if (workOrders == null || !workOrders.Any())
                return new List<WorkOrderDTO>();

            var dtos = workOrders.Select(workOrder =>
            {
                var firstSlot = workOrder.Slots?.OrderBy(s => s.StartTime).FirstOrDefault();
                var lastSlot = workOrder.Slots?.OrderByDescending(s => s.EndTime).FirstOrDefault();

                var startTime = firstSlot?.StartTime;

                double totalTasksDuration = workOrder.WorkOrderRepairTasks?.Sum(rt => rt.Duration ?? 0) ?? 0;

                TimeSpan endTime;
                if (startTime.HasValue)
                    endTime = startTime.Value.Add(TimeSpan.FromMinutes(totalTasksDuration));
                else
                    endTime = lastSlot?.EndTime ?? TimeSpan.Zero;

                var totalDuration = (int)(endTime - (startTime ?? TimeSpan.Zero)).TotalMinutes;

                return new WorkOrderDTO
                {
                    Code = $"WO-{workOrder.Id:D5}",
                    orderId = workOrder.Id,
                    Duration = totalDuration,
                    Scheduled = firstSlot != null ? firstSlot.ScheduleDay.Date : workOrder.StartTime.Date,
                    Status = workOrder.Status.ToString(),
                    TechnicianName = workOrder.Technician?.UserName ?? "Unassigned",
                    TecnicianId = workOrder.Technician?.Id ?? "0",
                    WorkStationName = firstSlot?.WorkStation?.Name ?? "Not assigned",
                    StartTime = (TimeSpan)startTime,
                    EndTime = endTime,
                    Invoice = workOrder.Invoice != null
                        ? new getInvoiceOrder
                        {
                            InvoiceId = workOrder.Invoice.Id,
                            IsPaid = workOrder.Invoice.IsPaid
                        }
                        : null,

                    Vehicle = new GetVehicleDTO
                    {
                        Id = workOrder.Vehicle.Id,
                        LicensePlate = workOrder.Vehicle.LicensePlate,
                        CustomerName = workOrder.Customer.Name,
                        VehicleModel = new VehicleModelDTO
                        {
                            Name = workOrder.Vehicle.VehicleModel?.Name ?? "N/A",
                            VehicleId = workOrder.Vehicle.Id,
                            Year = new YearModelDTO
                            {
                                NameYear = workOrder.Vehicle.VehicleModel.Year.YearName
                            },
                            Make = new MakeModelDTO
                            {
                                MakeName = workOrder.Vehicle.VehicleModel.Make.Name
                            }
                        },
                    },

                    RepairTasks = workOrder.WorkOrderRepairTasks.Select(rt => new RepairTaskDTO
                    {
                        Name = rt.RepairTask.Name,
                        Description = rt.RepairTask.Description,
                        LaborCost = rt.RepairTask.LaborCost,
                        Duration = rt.Duration ?? 0,
                        Parts = rt.RepairTask.Parts.Select(p => new PartDTO
                        {
                            Name = p.Name,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice
                        }).ToList()
                    }).ToList()
                };
            }).ToList();

            return dtos;
        }
        public async Task UpdateTechnicianForWorkOrderAsync(int orderId, string? newTechnicianId)
        {
            var order = await db.WorkOrders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order with this {orderId} Not Found");

            var technicianId = newTechnicianId ?? order.TechnicianId;
            if (string.IsNullOrEmpty(technicianId))
                throw new InvalidOperationException("Technician ID is required.");


            order.TechnicianId = technicianId;

            await db.SaveChangesAsync();
        }
        public async Task UpdateStationForWorkOrderAsync(List<int> slotIds, int newStationId)
        {
            var oldSlots = await db.ScheduleSlots
                                   .Where(s => slotIds.Contains(s.Id))
                                   .OrderBy(s => s.StartTime)
                                   .ToListAsync();

            if (!oldSlots.Any())
                throw new InvalidOperationException("No slots found for the provided IDs.");


            var oldStartTime = oldSlots.First().StartTime;
            var oldEndTime = oldSlots.Last().EndTime;
            var duration = oldEndTime - oldStartTime;


            var newSlot = await db.ScheduleSlots
                .FirstOrDefaultAsync(s => s.WorkStationId == newStationId
                                       && s.StartTime == oldStartTime);

            if (newSlot == null)
            {

                foreach (var oldSlot in oldSlots)
                {
                    oldSlot.WorkStationId = newStationId;

                }
            }
            else
            {

                foreach (var oldSlot in oldSlots)
                {
                    oldSlot.WorkStationId = newStationId;
                    oldSlot.StartTime = oldStartTime;
                    oldSlot.EndTime = oldStartTime.Add(duration);
                }
            }

            await db.SaveChangesAsync();
        }



        public async Task AddRepairTasksToWorkOrderAsync(int workOrderId, List<int> repairTaskIds)
        {
            var workOrder = await db.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException("Work order not found.");

            var existingTaskIds = workOrder.WorkOrderRepairTasks?
                .Select(wrt => wrt.RepairTaskId)
                .ToHashSet() ?? new HashSet<int>();

            int totalAddedDuration = 0;

            foreach (var repairTaskId in repairTaskIds)
            {
                if (existingTaskIds.Contains(repairTaskId))
                    continue;

                var repairTask = await db.RepairTasks.FindAsync(repairTaskId);
                if (repairTask == null)
                    throw new InvalidOperationException($"RepairTask with ID {repairTaskId} not found.");

                var newLink = new WorkOrderRepairTask
                {
                    WorkOrder = workOrder,
                    RepairTask = repairTask,
                    Duration = repairTask.EstimatedDuration
                };

                totalAddedDuration += repairTask.EstimatedDuration;
                workOrder.WorkOrderRepairTasks.Add(newLink);
            }

        
            if (totalAddedDuration > 0)
            {
                workOrder.EndTime = workOrder.EndTime.AddMinutes(totalAddedDuration);
            }

            await db.SaveChangesAsync();
        }


        public async Task<List<WorkOrderDTO>> GetAllWorkOrdersAsync()
        {
            var workOrders = await db.WorkOrders.ToListAsync();

            if (workOrders == null || !workOrders.Any())
                return new List<WorkOrderDTO>();

            var dtos = workOrders.Select(workOrder =>
            {
                var firstSlot = workOrder.Slots?.OrderBy(s => s.StartTime).FirstOrDefault();
                var lastSlot = workOrder.Slots?.OrderByDescending(s => s.EndTime).FirstOrDefault();

                var startTime = firstSlot?.StartTime;

                double totalTasksDuration = workOrder.WorkOrderRepairTasks?.Sum(rt => rt.Duration ?? 0) ?? 0;

                TimeSpan endTime;
                if (startTime.HasValue)
                    endTime = startTime.Value.Add(TimeSpan.FromMinutes(totalTasksDuration));
                else
                    endTime = lastSlot?.EndTime ?? TimeSpan.Zero;

                var totalDuration = (int)(endTime - (startTime ?? TimeSpan.Zero)).TotalMinutes;

                return new WorkOrderDTO
                {
                    Code = $"WO-{workOrder.Id:D5}",
                    orderId = workOrder.Id,
                    Duration = totalDuration,
                    Scheduled = firstSlot != null ? firstSlot.ScheduleDay.Date : workOrder.StartTime.Date,
                    Status = workOrder.Status.ToString(),
                    TechnicianName = workOrder.Technician?.UserName ?? "Unassigned",
                    TecnicianId = workOrder.Technician?.Id ?? "0",
                    WorkStationName = firstSlot?.WorkStation?.Name ?? "Not assigned",
                    StartTime = (TimeSpan)startTime,
                    EndTime = endTime,
                    Invoice = workOrder.Invoice != null
                                 ? new getInvoiceOrder
                                    {
                                        InvoiceId = workOrder.Invoice.Id,
                                        IsPaid = workOrder.Invoice.IsPaid
                                    }
                                    : null,

                    Vehicle = new GetVehicleDTO
                    {
                        Id = workOrder.Vehicle.Id,
                        LicensePlate = workOrder.Vehicle.LicensePlate,
                        CustomerName = workOrder.Customer.Name,
                        VehicleModel = new VehicleModelDTO
                        {
                            Name = workOrder.Vehicle.VehicleModel?.Name ?? "N/A",
                            VehicleId = workOrder.Vehicle.Id,
                            Year = new YearModelDTO
                            {
                                NameYear = workOrder.Vehicle.VehicleModel.Year.YearName
                            },
                            Make = new MakeModelDTO
                            {
                                MakeName = workOrder.Vehicle.VehicleModel.Make.Name
                            }

                        },
                    },

                    RepairTasks = workOrder.WorkOrderRepairTasks.Select(rt => new RepairTaskDTO
                    {
                        Name = rt.RepairTask.Name,
                        Description = rt.RepairTask.Description,
                        LaborCost = rt.RepairTask.LaborCost,
                        Duration = rt.Duration ?? 0,
                        Parts = rt.RepairTask.Parts.Select(p => new PartDTO
                        {
                            Name = p.Name,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice
                        }).ToList()
                    }).ToList()
                };
            }).ToList();

            return dtos;
        }
        public async Task<bool> DeletWorkOrder(int orderId)
        {
            var workOrder = await db.WorkOrders
                .Include(w => w.Slots)
                .FirstOrDefaultAsync(w => w.Id == orderId);

            if (workOrder == null)
                throw new KeyNotFoundException("Work order not found.");

            
            foreach (var slot in workOrder.Slots)
            {
                slot.WorkOrderId = null;
                slot.IsAvailable = true;  
                slot.TechnicianId = null;
            }

            db.WorkOrders.Remove(workOrder);
            await db.SaveChangesAsync();

            return true;
        }

    }
}
