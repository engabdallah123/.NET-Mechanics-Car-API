using Data;
using Domain.DTO;
using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.DTO.Schedule_Day;
using Domain.DTO.Schedule_Slot;
using Domain.DTO.Vehicle;
using Domain.DTO.Vehicle_Model;
using Domain.DTO.Work_Order;
using Domain.DTO.Work_Station;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class ScheduleRepo : IScheduleRepo
    {
        private readonly DataContext db;
        private readonly IMemoryCache cache;

        public ScheduleRepo(DataContext db,IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }
        public async Task<ScheduleDayDTO> GetOrCreateScheduleForTodayAsync()
        {
            var today = DateTime.Today;

            // if the day exist => return it
            var existingDay = await db.ScheduleDays
                .FirstOrDefaultAsync(d => d.Date == today);

            if (existingDay == null)
            {
                existingDay = await GenerateDayScheduleAsync(today);
            }

            var cacheKey = "Schedule_Day";
            if(!cache.TryGetValue(cacheKey, out ScheduleDayDTO scheduleDay))
            {
                var currentTime = existingDay.Date == DateTime.Today ? DateTime.Now.TimeOfDay : TimeSpan.Zero;


                var result = new ScheduleDayDTO
                {
                    Date = existingDay.Date,
                    Stations = existingDay.Slots
                     .GroupBy(s => s.WorkStation)
                     .Select(g => new WorkStationDTO
                     {
                         Name = g.Key.Name,
                         Code = g.Key.Code,
                         stactionId = g.Key.Id,
                         Slots = g.Select(slot =>
                         {
                             // إذا فيه WorkOrder مربوط بالـ slot
                             var workOrder = slot.WorkOrder;
                             int totalDuration = workOrder != null
                                 ? (int)(workOrder.EndTime - workOrder.StartTime).TotalMinutes
                                 : 0;

                             WorkOrderDTO workOrderDto = null;
                             if (workOrder != null)
                             {
                                 var firstSlot = workOrder.Slots?.OrderBy(s => s.StartTime).FirstOrDefault();

                                 workOrderDto = new WorkOrderDTO
                                 {
                                     Code = $"WO-{workOrder.Id:D5}",
                                     orderId = workOrder.Id,
                                     Duration = totalDuration,
                                     Scheduled = firstSlot?.ScheduleDay.Date ?? workOrder.StartTime.Date,
                                     Status = workOrder.Status.ToString(),
                                     TechnicianName = workOrder.Technician?.UserName ?? "Unassigned",
                                     TecnicianId = workOrder.Technician?.Id ?? "0",
                                     WorkStationName = firstSlot?.WorkStation?.Name ?? "Not assigned",
                                     Vehicle = workOrder.Vehicle != null ? new GetVehicleDTO
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
                                                 NameYear = workOrder.Vehicle.VehicleModel?.Year?.YearName ?? "N/A"
                                             },
                                             Make = new MakeModelDTO
                                             {
                                                 MakeName = workOrder.Vehicle.VehicleModel?.Make?.Name ?? "N/A"
                                             }
                                         }
                                     } : null,
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
                             }

                             return new ScheduleSlotDTO
                             {
                                 slotId = slot.Id,
                                 StartTime = slot.StartTime,
                                 EndTime = slot.EndTime,
                                 IsAvailable = slot.IsAvailable,
                                 WorkOrderDTO = workOrderDto,
                                 InPast = slot.EndTime < currentTime
                             };
                         }).ToList()
                     }).ToList()
                };

                scheduleDay = result;
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                cache.Set(cacheKey, scheduleDay,options);
                return scheduleDay;
            }
            return scheduleDay;
            
        }
        public async Task<ScheduleDay> GenerateDayScheduleAsync(DateTime date)
        {
            var stations = await db.WorkStations.ToListAsync();
            if (!stations.Any())
                throw new InvalidOperationException("No WorkStations found in the system.");

            var scheduleDay = new ScheduleDay
            {
                Date = date,
            };

            db.ScheduleDays.Add(scheduleDay);
            await db.SaveChangesAsync();

            // Setting time
            var startOfDay = new TimeSpan(8, 0, 0); // 8:00 AM
            var endOfDay = new TimeSpan(23, 0, 0);  // 11:00 PM
            var slotDuration = TimeSpan.FromMinutes(15);

            foreach (var station in stations)
            {
                var currentTime = startOfDay;
                while (currentTime < endOfDay)
                {
                    var slot = new ScheduleSlot
                    {
                        ScheduleDayId = scheduleDay.Id,
                        WorkStationId = station.Id,
                        StartTime = currentTime,
                        EndTime = currentTime.Add(slotDuration),
                        IsAvailable = true

                    };
                    db.ScheduleSlots.Add(slot);
                    currentTime = currentTime.Add(slotDuration);
                }
            }

            await db.SaveChangesAsync();
            return scheduleDay;
        }
        public async Task<List<WorkStationScheduleDTO>> GetTodayScheduleAsync()
        {
            var today = DateTime.Today;

            var stations = await db.WorkStations.ToListAsync();

            var cacheKey = "Schedule_Day";
            if (!cache.TryGetValue(cacheKey, out List<WorkStationScheduleDTO> scheduleDay))
            {
                var result = stations.Select(ws => new WorkStationScheduleDTO
                {
                    WorkStationId = ws.Id,
                    WorkStationName = ws.Name,
                    Code = ws.Code ?? string.Empty,
                    Slots = ws.Slots
                   .Where(s => s.ScheduleDay.Date.Date == today)
                   .OrderBy(s => s.StartTime)
                   .Select(s => new SlotStatusDTO
                   {
                       SlotId = s.Id,
                       StartTime = s.StartTime,
                       EndTime = s.EndTime,
                       TechnicianName = s.Technician?.UserName,
                       IsAvailable = s.IsAvailable,
                       WorkOrderId = s.WorkOrderId,
                       Status = s.WorkOrder != null ? s.WorkOrder.Status.ToString() : "Empty"
                   }).ToList()
                }).ToList();

                scheduleDay = result;
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                cache.Set(cacheKey, scheduleDay, options);
                return scheduleDay;
            }
            return scheduleDay;

        }
        public async Task<bool> ReScheduleWorkOrderAsync(int workOrderId,List<int> newSlotIds,string? newTechnicianId = null)
        {
            var workOrder = await db.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException("WorkOrder not found.");

            var newSlots = await db.ScheduleSlots
                .Where(s => newSlotIds.Contains(s.Id))
                .ToListAsync();

            if (newSlots.Count != newSlotIds.Count || newSlots.Any(s => !s.IsAvailable))
                throw new InvalidOperationException("Some slots are not available.");

            // فك الارتباط من القديمة
            foreach (var slot in workOrder.Slots)
            {
                slot.IsAvailable = true;
                slot.WorkOrderId = null;
                slot.TechnicianId = null;
            }

            // ربط الجديدة
            foreach (var slot in newSlots)
            {
                slot.WorkOrderId = workOrder.Id;
                slot.TechnicianId = newTechnicianId ?? workOrder.TechnicianId;
                slot.IsAvailable = false;
            }

            // تحديث بيانات الأوردر بناءً على أول وآخر Slot
            var firstSlot = newSlots.MinBy(s => s.StartTime);
            var lastSlot = newSlots.MaxBy(s => s.EndTime);

            workOrder.StartTime = firstSlot.ScheduleDay.Date.Add(firstSlot.StartTime);
            workOrder.EndTime = lastSlot.ScheduleDay.Date.Add(lastSlot.EndTime);
            workOrder.TechnicianId = newTechnicianId ?? workOrder.TechnicianId;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<ScheduleDayDTO> GetScheduleDayWithWorkOrdersAsync(DateTime date)
        {
            // جلب اليوم الحالي مع كل الـ slots والـ workorders المرتبطة
            var existingDay = await db.ScheduleDays
                .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

            if (existingDay == null) throw new KeyNotFoundException("Schedule day not found.");

            var currentTime = existingDay.Date == DateTime.Today ? DateTime.Now.TimeOfDay : TimeSpan.Zero;

            var scheduleDayDto = new ScheduleDayDTO
            {
                Date = existingDay.Date,
                Stations = existingDay.Slots
                    .GroupBy(s => s.WorkStation)
                    .Select(g => new WorkStationDTO
                    {
                        Name = g.Key.Name,
                        Code = g.Key.Code,
                        stactionId = g.Key.Id,
                        Slots = g.Select(slot =>
                        {
                            // إذا فيه WorkOrder مربوط بالـ slot
                            var workOrder = slot.WorkOrder;
                            int totalDuration = workOrder != null
                                ? (int)(workOrder.EndTime - workOrder.StartTime).TotalMinutes
                                : 0;

                            WorkOrderDTO workOrderDto = null;
                            if (workOrder != null)
                            {
                                var firstSlot = workOrder.Slots?.OrderBy(s => s.StartTime).FirstOrDefault();

                                workOrderDto = new WorkOrderDTO
                                {
                                    Code = $"WO-{workOrder.Id:D5}",
                                    orderId = workOrder.Id,
                                    Duration = totalDuration,
                                    Scheduled = firstSlot?.ScheduleDay.Date ?? workOrder.StartTime.Date,
                                    Status = workOrder.Status.ToString(),
                                    TechnicianName = workOrder.Technician?.UserName ?? "Unassigned",
                                    TecnicianId = workOrder.Technician?.Id ?? "0",
                                    WorkStationName = firstSlot?.WorkStation?.Name ?? "Not assigned",
                                    Vehicle = workOrder.Vehicle != null ? new GetVehicleDTO
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
                                                NameYear = workOrder.Vehicle.VehicleModel?.Year?.YearName ?? "N/A"
                                            },
                                            Make = new MakeModelDTO
                                            {
                                                MakeName = workOrder.Vehicle.VehicleModel?.Make?.Name ?? "N/A"
                                            }
                                        }
                                    } : null,
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
                            }

                            return new ScheduleSlotDTO
                            {
                                slotId = slot.Id,
                                StartTime = slot.StartTime,
                                EndTime = slot.EndTime,
                                IsAvailable = slot.IsAvailable,
                                WorkOrderDTO = workOrderDto,
                                InPast = slot.EndTime < currentTime
                            };
                        }).ToList()
                    }).ToList()
            };

            return scheduleDayDto;
        }


    }
}

